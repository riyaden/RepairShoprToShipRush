using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Microsoft.Extensions.Logging;
using RepairShoprToShipRush.Domain;
using RepairShoprToShipRush.Helpers;
using System.Linq;
using System.Collections.Generic;

namespace RepairShoprToShipRush.Connectors
{
    class ShipRushConnector : IDisposable
    {
        private HttpClient client;
        private ILogger log;

        public ShipRushConnector(ILogger log)
        {
            this.log = log;

            client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public async Task<string> AddOrder(string uri, Invoice invoice)
        {
            string lineitemsXml = GetLineItems(Constants.itemPayloadTemplate, invoice);
            string xmlPayload = GetXmlContent(Constants.xmlPayloadTemplate, invoice, lineitemsXml);
            var xmlContent = new StringContent(xmlPayload, Encoding.UTF8, "application/xml");

            log.LogInformation($"{DateTime.Now} | Pushing payload {xmlPayload} to Uri {uri}");

            var response = await client.PostAsync(uri, xmlContent);

            if (response.IsSuccessStatusCode)
            {
                var orderXml = await response.Content.ReadAsStringAsync();
                XmlDocument orderDocument = new XmlDocument();
                orderDocument.LoadXml(orderXml);

                try
                {
                    return orderDocument.GetElementsByTagName("OrderId")[0].InnerText;
                }
                catch (Exception)
                {
                    log.LogWarning($"{DateTime.Now} | The response from ShipRush did not return an OrderId, most likely this order already exists.");
                    return "null-order-id";
                }
            }

            return null;
        }

        private string GetLineItems(string itemPayloadTemplate, Invoice invoice)
        {
            string lineitems = string.Empty;

            foreach (var lineitem in invoice.line_items)
            {
                string itemtotal = (float.Parse(lineitem.quantity) * float.Parse(lineitem.price)).ToString();

                string itemPayload = string.Format(itemPayloadTemplate,
                    HttpUtility.HtmlEncode(lineitem.name),
                    lineitem.price,
                    lineitem.quantity,
                    itemtotal
                    );

                lineitems += itemPayload;
            }

            return lineitems;
        }

        private string GetState(Dictionary<string, string> dictionary, string state, string country)
        {
            foreach (string key in dictionary.Keys)
            {
                if (dictionary[key].ToUpper() == state.ToUpper())
                {
                    return country;
                }

            }
            return "";
        }
        
        private string GetXmlContent(string xmlPayloadTemplate, Invoice invoice, string lineitems)
        {

            string[] statecountry = invoice.customer.state.ToUpper().Split(',');
            string[] statesList = Constants.statesList.ToUpper().Split(',');
            string[] countriesList = Constants.countriesList.ToUpper().Split(',');
            
            string[] statesUKList = Constants.statesUKList.ToUpper().Split(',');

            string stateNamesList = Constants.stateNamesList.ToUpper();

            string state = string.Empty;
            string country = string.Empty;

            Dictionary<string, string> dictUSSates = stateNamesList.ToUpper().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                     .Select(part => part.Split('='))
                     .ToDictionary(split => split[0].ToUpper(), split => split[1].ToUpper());

            state = statecountry[0].Trim().ToUpper();
            if (statecountry.Length == 1)
            {
                bool stateExists = Array.Exists(statesList,
                                                delegate (string s) {
                                                    return s.Equals(state);
                                                });


                if (stateExists)
                    country = "US";
                else
                {
                    // Check long state
                    country = GetState(dictUSSates, state, "US");

                    if (country == "")
                    {
                        // Get UK States
                        stateExists = Array.Exists(statesUKList,
                                              delegate (string s) {
                                                  return s.Equals(state);
                                              });
                        if (stateExists)
                            country = "UK";
                        else
                            country = state;

                    }
                  
                }

            }
            else if (statecountry.Length > 1)
            {
                // Check long state
                country = GetState(dictUSSates, state, "US");
                if (country == "")
                {
                    // Get UK States
                    bool stateExists = Array.Exists(statesUKList,
                                          delegate (string s) {
                                              return s.Equals(state);
                                          });
                    if (stateExists)
                        country = "UK";
                    else
                        country = state;

                }

            }
            else
            {
                state = statecountry[0].Trim();
                country = "US";

            }        

            string xmlPayload = string.Format(xmlPayloadTemplate,
                                                    HttpUtility.HtmlEncode(invoice.customer.fullname),
                                                    HttpUtility.HtmlEncode(invoice.customer.business_name),
                                                    HttpUtility.HtmlEncode(invoice.customer.address),
                                                    HttpUtility.HtmlEncode(invoice.customer.address_2),
                                                    HttpUtility.HtmlEncode(invoice.customer.city),
                                                    state,
                                                    country,
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
}
