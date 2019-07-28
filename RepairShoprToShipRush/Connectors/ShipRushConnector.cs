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
using Places;

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
            string xmlPayload = await GetXmlContentAsync(Constants.xmlPayloadTemplate, invoice, lineitemsXml);
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

        private string GetCountry(Dictionary<string, string> dictionary, string country)
        {
            foreach (string key in dictionary.Keys)
            {
                if (dictionary[key].ToUpper() == country.ToUpper())
                {
                    return key;
                }

            }
            return "";
        }
        
        private async Task<string> GetXmlContentAsync(string xmlPayloadTemplate, Invoice invoice, string lineitems)
        {
            // Response results;
            //string city = invoice.customer.city;



            string[] statecountry = invoice.customer.state.ToUpper().Split(',');
            string countryList = Constants.countryFull.ToUpper();

            string state = string.Empty;
            string country = string.Empty;

            state = statecountry[0].Trim().ToUpper();

            Response results;
            var placeList = new List<Place>();
            string city = invoice.customer.city;

            results = await Places.Api.TextSearch(city + ", " + state, Constants.apiKey);
            //add the results to placeList
            foreach (var place in results.Places)
            {
                placeList.Add(place);
            }

            string address = "";
            foreach (var place in placeList)
            {
                var placeDetails = await Places.Api.GetDetails(place.PlaceId, Constants.apiKey);
               
                string name = place.Name;
                address = placeDetails.Address;
                break;

            }
            string countryFull = address.Substring(address.LastIndexOf(",")).Trim().ToUpper();

            Dictionary<string, string> dictCountries = countryList.ToUpper().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                     .Select(part => part.Split('='))
                     .ToDictionary(split => split[0].ToUpper(), split => split[1].ToUpper());



            if (countryFull.Length > 3)
            {
                country = GetCountry(dictCountries, countryFull);
            }
            else
            {
                countryList = Constants.countryThree.ToUpper();

                dictCountries = countryList.ToUpper().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(part => part.Split('='))
                                .ToDictionary(split => split[0].ToUpper(), split => split[1].ToUpper());

                country = GetCountry(dictCountries, countryFull);

            }

            /*
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

    */
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
