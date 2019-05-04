using System;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Xml;

namespace RepairShoprToShipRush
{
    public static class RepairShoprToShipRush
    {
        [FunctionName("RepairShoprToShipRush")]
        public static async Task RunAsync([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"{DateTime.Now} | C# Timer trigger function has started");

            const string xmlPayloadTemplate = "<?xml version = \"1.0\"?><Request><ShipTransaction><Shipment><Package><PackageActualWeight>0</PackageActualWeight></Package><DeliveryAddress><Address><FirstName>{0}</FirstName><Company>{1}</Company><Address1>{2}</Address1><Address2>{3}</Address2><City>{4}</City><State>{5}</State><Country>US</Country><StateAsString>{5}</StateAsString><CountryAsString>US</CountryAsString><PostalCode>{6}</PostalCode><Phone>{7}</Phone><EMail>{8}</EMail></Address></DeliveryAddress></Shipment><Order>{9}<OrderNumber>{10}</OrderNumber><PaymentStatus>2</PaymentStatus><ItemsTax>{11}</ItemsTax><Total>{12}</Total><ItemsTotal>{13}</ItemsTotal><BillingAddress><FirstName>{0}</FirstName><Company>{1}</Company><Address1>{2}</Address1><Address2>{3}</Address2><City>{4}</City><State>{5}</State><Country>US</Country><StateAsString>{5}</StateAsString><CountryAsString>US</CountryAsString><PostalCode>{6}</PostalCode><Phone>{7}</Phone><EMail>{8}</EMail></BillingAddress><ShippingAddress><FirstName>{0}</FirstName><Company>{1}</Company><Address1>{2}</Address1><Address2>{3}</Address2><City>{4}</City><State>{5}</State><Country>US</Country><StateAsString>{5}</StateAsString><CountryAsString>US</CountryAsString><PostalCode>{6}</PostalCode><Phone>{7}</Phone><EMail>{8}</EMail></ShippingAddress></Order></ShipTransaction></Request>";
            const string itemPayloadTemplate = "<ShipmentOrderItem><Name>{0}</Name><Price>{1}</Price><Quantity>{2}</Quantity><Total>{3}</Total></ShipmentOrderItem>";

            string repairShoprUri = Environment.GetEnvironmentVariable("repairShoprUri");
            string repairShoprApiKey = Environment.GetEnvironmentVariable("repairShoprApiKey");
            string shipRushUri = Environment.GetEnvironmentVariable("shipRushUri");

            using (var repairShoprClient = new HttpClient())
            {
                repairShoprClient.DefaultRequestHeaders.Accept.Clear();
                repairShoprClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var invoicesUri = repairShoprUri + "?api_key=" + repairShoprApiKey;
                log.LogInformation($"{DateTime.Now} | Getting invoices from Uri {invoicesUri}");
                var invoicesResponse = await repairShoprClient.GetAsync(invoicesUri);

                if (invoicesResponse.IsSuccessStatusCode)
                {
                    var invoicesJson = await invoicesResponse.Content.ReadAsStringAsync();
                    var invoicesRoot = JsonConvert.DeserializeObject<Invoices>(invoicesJson);
                    var invoices = invoicesRoot.invoices
                        .Where(i => i.is_paid)
                        .Where(delegate (Invoice i)
                        {
                            return
                            (string.IsNullOrEmpty(i.note))
                            ||
                            (!string.IsNullOrEmpty(i.note) && !i.note.Contains("ShipRushOrderID#"));
                        });

                    log.LogInformation($"{DateTime.Now} | Found {invoices.Count()} new invoices");

                    if (invoices.Count() > 0)
                    {
                        using (var shipRushClient = new HttpClient())
                        {
                            shipRushClient.DefaultRequestHeaders.Accept.Clear();
                            shipRushClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                            foreach (var invoiceItem in invoices)
                            {
                                var invoiceDetailsUri = repairShoprUri + "/" + invoiceItem.id + "?api_key=" + repairShoprApiKey;
                                log.LogInformation($"{DateTime.Now} | Getting invoices from Uri {invoiceDetailsUri}");
                                var invoiceResponse = await repairShoprClient.GetAsync(invoiceDetailsUri);

                                if (invoiceResponse.IsSuccessStatusCode)
                                {
                                    var invoiceJson = await invoiceResponse.Content.ReadAsStringAsync();
                                    var invoice = JsonConvert.DeserializeObject<InvoiceObject>(invoiceJson).invoice;

                                    log.LogInformation($"{DateTime.Now} | Parsing invoice, wish me luck!");

                                    string lineitems = GetLineItems(itemPayloadTemplate, invoice);

                                    string xmlPayload = GetXmlContent(log, xmlPayloadTemplate, invoice, lineitems);

                                    var xmlContent = new StringContent(xmlPayload, Encoding.UTF8, "application/xml");

                                    log.LogInformation($"{DateTime.Now} | Pushing payload to Uri {shipRushUri}");
                                    var orderResponse = await shipRushClient.PostAsync(shipRushUri, xmlContent);

                                    if (orderResponse.IsSuccessStatusCode)
                                    {
                                        var orderXml = await orderResponse.Content.ReadAsStringAsync();
                                        XmlDocument orderDocument = new XmlDocument();
                                        orderDocument.LoadXml(orderXml);
                                        var orderId = "null-order-id";

                                        try
                                        {
                                            orderId = orderDocument.GetElementsByTagName("OrderId")[0].InnerText;
                                        }
                                        catch (Exception)
                                        {
                                            log.LogWarning($"{DateTime.Now} | The response from ShipRush did not return an OrderId, most likely this order already exists.");
                                        }

                                        log.LogInformation($"{DateTime.Now} | Updating invoice with shipping OrderId to leave our mark {orderId}");

                                        var noteContent = new StringContent("{\"note\":\"ShipRushOrderID#" + orderId + @"\r\n" + invoice.note + "\"}", Encoding.UTF8, "application/json");

                                        var orderUpdate = await repairShoprClient.PutAsync(invoiceDetailsUri, noteContent);

                                        if (orderUpdate.IsSuccessStatusCode)
                                        {
                                            var result = await orderUpdate.Content.ReadAsStringAsync();
                                            log.LogInformation($"{DateTime.Now} | Invoice updated: {result}");
                                        }
                                        else
                                        {
                                            log.LogError($"{DateTime.Now} | Updating invoice has probably failed, this could result in this invoice being picked up again, please cleanup this invoice manually from repairshopr");
                                        }
                                    }
                                    else
                                    {
                                        log.LogError($"{DateTime.Now} | Pushing payload to Uri {shipRushUri} has probably failed.");
                                    }
                                }
                                else
                                {
                                    log.LogError($"{DateTime.Now} | Failed to get invoice details from Uri {invoiceDetailsUri}, Error details: {invoiceResponse.Content}");
                                }
                            }
                        }
                    }
                    else
                    {
                        log.LogInformation($"{DateTime.Now} | No new invoices found, nothing to do here...");
                    }
                }
                else
                {
                    log.LogError($"{DateTime.Now} | Failed to get invoices from Uri {invoicesUri}, Error details: {invoicesResponse.Content}");
                }
            }

            log.LogInformation($"{DateTime.Now} | C# Timer trigger function has ended");
        }

        private static string GetLineItems(string itemPayloadTemplate, Invoice invoice)
        {
            string lineitems = string.Empty;

            foreach (var lineitem in invoice.line_items)
            {
                string itemtotal = (float.Parse(lineitem.quantity) * float.Parse(lineitem.price)).ToString();

                string itemPayload = string.Format(itemPayloadTemplate,
                    lineitem.name,
                    lineitem.price,
                    lineitem.quantity,
                    itemtotal
                    );

                lineitems += itemPayload;
            }

            return lineitems;
        }

        private static string GetXmlContent(ILogger log, string xmlPayloadTemplate, Invoice invoice, string lineitems)
        {
            string xmlPayload = string.Format(xmlPayloadTemplate,
                                                    invoice.customer.fullname,
                                                    invoice.customer.business_name,
                                                    invoice.customer.address,
                                                    invoice.customer.address_2,
                                                    invoice.customer.city,
                                                    invoice.customer.state,
                                                    invoice.customer.zip,
                                                    invoice.customer.mobile,
                                                    invoice.customer.email,
                                                    lineitems,
                                                    invoice.number,
                                                    invoice.tax,
                                                    invoice.total,
                                                    invoice.line_items.Count
                                                    );

            log.LogInformation($"{DateTime.Now} | Parsing completed, XML Payload: {xmlPayload}");
            return xmlPayload;
        }
    }

    public class LineItem
    {
        public int? id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int? invoice_id { get; set; }
        public string item { get; set; }
        public string name { get; set; }
        public string cost { get; set; }
        public string price { get; set; }
        public string quantity { get; set; }
        public object product_id { get; set; }
        public bool? taxable { get; set; }
        public object discount_percent { get; set; }
        public int? position { get; set; }
        public object invoice_bundle_id { get; set; }
        public string product_category { get; set; }
        public object tax_note { get; set; }
        public int? user_id { get; set; }
        public object line_discount_percent { get; set; }
        public object discount_dollars { get; set; }
    }

    public class Meta
    {
        public int total_pages { get; set; }
        public int page { get; set; }
    }

    public class Invoices
    {
        public List<Invoice> invoices { get; set; }
        public Meta meta { get; set; }
    }

    public class Payment
    {
        public int id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public bool success { get; set; }
        public float payment_amount { get; set; }
        public List<int> invoice_ids { get; set; }
        public string ref_num { get; set; }
        public string applied_at { get; set; }
        public string payment_method { get; set; }
        public object transaction_response { get; set; }
    }

    public class Customer
    {
        public int id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string fullname { get; set; }
        public string business_name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object pdf_url { get; set; }
        public string address { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public object latitude { get; set; }
        public object longitude { get; set; }
        public object notes { get; set; }
        public bool get_sms { get; set; }
        public bool opt_out { get; set; }
        public bool disabled { get; set; }
        public bool no_email { get; set; }
        public object location_name { get; set; }
        public object location_id { get; set; }
        public string online_profile_url { get; set; }
        public int? tax_rate_id { get; set; }
        public string notification_email { get; set; }
        public string invoice_cc_emails { get; set; }
        public object invoice_term_id { get; set; }
        public string referred_by { get; set; }
        public object ref_customer_id { get; set; }
        public string business_and_full_name { get; set; }
        public string business_then_name { get; set; }
        public List<object> contacts { get; set; }
    }

    public class Invoice
    {
        public int id { get; set; }
        public string number { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string date { get; set; }
        public string due_date { get; set; }
        public string subtotal { get; set; }
        public string total { get; set; }
        public string tax { get; set; }
        public bool verified_paid { get; set; }
        public bool tech_marked_paid { get; set; }
        public int? ticket_id { get; set; }
        public object pdf_url { get; set; }
        public string balance_due { get; set; }
        public string note { get; set; }
        public List<LineItem> line_items { get; set; }
        public bool is_paid { get; set; }
        public object location_id { get; set; }
        public object contact_id { get; set; }
        public string po_number { get; set; }
        public List<Payment> payments { get; set; }
        public string hardwarecost { get; set; }
        public Customer customer { get; set; }
    }

    public class InvoiceObject
    {
        public Invoice invoice { get; set; }
    }
}