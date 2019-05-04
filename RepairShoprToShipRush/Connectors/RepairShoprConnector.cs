using Newtonsoft.Json;
using RepairShoprToShipRush.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RepairShoprToShipRush.Connectors
{
    class RepairShoprConnector : IDisposable
    {
        private HttpClient client;
        private ILogger log;

        public RepairShoprConnector(ILogger log)
        {
            this.log = log;


            client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public async Task<List<Invoice>> GetInvoicesList(string uri, Func<Invoice, bool> filter = null)
        {
            log.LogInformation($"{DateTime.Now} | Getting invoices from Uri {uri}");

            var response = await client.GetAsync(uri);

            var result = new List<Invoice>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var invoicesList = JsonConvert.DeserializeObject<InvoicesList>(json);
                result = invoicesList.invoices.Where(filter ?? (s => true)).ToList();
            }
            else
            {
                log.LogError($"{DateTime.Now} | Failed to get invoices from Uri {uri}, Error details: {response.Content}");
            }

            return result;
        }

        public async Task<Invoice> GetInvoice(string uri)
        {
            var response = await client.GetAsync(uri);

            Invoice invoice = null;

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                invoice = JsonConvert.DeserializeObject<InvoiceWrapper>(json)?.invoice;
            }

            return invoice;
        }

        public async Task<Invoice> SetInvoice(string uri, string changesJson)
        {
            var response = await client.PutAsync(uri, new StringContent(changesJson, Encoding.UTF8, "application/json"));

            Invoice invoice = null;

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                invoice = JsonConvert.DeserializeObject<InvoiceWrapper>(json)?.invoice;

                log.LogInformation($"{DateTime.Now} | Invoice updated: {json}");
            }

            return invoice;
        }
    }
}
