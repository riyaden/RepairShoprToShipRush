using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using RepairShoprToShipRush.Domain;
using RepairShoprToShipRush.Connectors;

namespace RepairShoprToShipRush
{
    public static class RepairShoprToShipRush
    {
        [FunctionName("RepairShoprToShipRush")]
        public static async Task RunAsync([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"{DateTime.Now} | C# Timer trigger function has started");

            string rsUri, rsApiKey, srUri, itemCode;
            bool testMode;

            if (!CheckEnvironmentVariables(out rsUri, out rsApiKey, out srUri, out itemCode, out testMode))
            {
                log.LogInformation($"{DateTime.Now} | Error reading environment variables, make sure application settings are correct");
                return;
            }

            using (var rsConnector = new RepairShoprConnector(log))
            {
                var invoicesListUri = rsUri + "?api_key=" + rsApiKey;
                var invoicesList = await rsConnector.GetInvoicesList(invoicesListUri, delegate (Invoice i)
                {
                    return
                    (i.is_paid.HasValue && i.is_paid.Value)
                    &&
                    ((string.IsNullOrEmpty(i.note))
                    ||
                    (!string.IsNullOrEmpty(i.note) && !i.note.Contains("ShipRushOrderID#")));
                });


                if (invoicesList.Count() == 0)
                {
                    log.LogInformation($"{DateTime.Now} | No new invoices found, nothing to do here...");
                    return;
                }

                log.LogInformation($"{DateTime.Now} | Found {invoicesList.Count()} new invoices");

                using (var srConnector = new ShipRushConnector(log))
                {
                    foreach (var item in invoicesList)
                    {
                        log.LogInformation($"{DateTime.Now} | Processing invoice {item.number}...");

                        var invoiceDetailsUri = rsUri + "/" + item.id + "?api_key=" + rsApiKey;
                        var invoice = await rsConnector.GetInvoice(invoiceDetailsUri);

                        if (invoice == null)
                        {
                            log.LogError($"{DateTime.Now} | Failed to get invoice details from Uri {invoiceDetailsUri}");
                            continue;
                        }

                        if (!string.IsNullOrEmpty(itemCode) && !invoice.line_items.Any(i => i.item.ToLower().Contains(itemCode.ToLower())))
                        {
                            log.LogInformation($"{DateTime.Now} | Invoice is not mail in. So we'll just leave our flag in it and exit");

                            var regularInvoice = await rsConnector.SetInvoice(invoiceDetailsUri, "{\"note\":\"ShipRushOrderID#null-order-id   " + invoice.note + "\"}");
                            if (regularInvoice == null || regularInvoice.note == invoice.note)
                            {
                                log.LogError($"{DateTime.Now} | Updating invoice has probably failed, this could result in this invoice being picked up again, please cleanup this invoice manually from RepairShopr");
                            }
                            continue;
                        }

                        if (testMode)
                        {
                            log.LogInformation($"{DateTime.Now} | Test mode active, no changes will be done...");
                            continue;
                        }

                        log.LogInformation($"{DateTime.Now} | Creating order on ShipRush...");
                        var orderId = await srConnector.AddOrder(srUri, invoice);

                        if (orderId == null)
                        {
                            log.LogError($"{DateTime.Now} | Creating order on ShipRush failed.");
                            continue;
                        }

                        log.LogInformation($"{DateTime.Now} | Updating invoice with shipping OrderId to leave our mark {orderId}");
                        var updatedInvoice = await rsConnector.SetInvoice(invoiceDetailsUri, "{\"note\":\"ShipRushOrderID#" + orderId + "   " + invoice.note + "\"}");
                        if (updatedInvoice == null || updatedInvoice.note == invoice.note)
                        {
                            log.LogError($"{DateTime.Now} | Updating invoice has probably failed, this could result in this invoice being picked up again, please cleanup this invoice manually from RepairShopr");
                        }
                    }
                }
            }

            log.LogInformation($"{DateTime.Now} | C# Timer trigger function has ended");
        }

        private static bool CheckEnvironmentVariables(out string rsUri, out string rsApiKey, out string srUri, out string itemCode, out bool testMode)
        {
            try
            {
                rsUri = Environment.GetEnvironmentVariable("repairShoprUri");
                rsApiKey = Environment.GetEnvironmentVariable("repairShoprApiKey");
                srUri = Environment.GetEnvironmentVariable("shipRushUri");
                itemCode = Environment.GetEnvironmentVariable("itemCode");
            }
            catch(Exception)
            {
                rsUri = null;
                rsApiKey = null;
                srUri = null;
                itemCode = null;
                testMode = false;

                return false;
            }

            try
            {
                testMode = bool.Parse(Environment.GetEnvironmentVariable("testMode"));
            }
            catch(Exception)
            {
                testMode = false;
            }

            return true;
        }
    }
}