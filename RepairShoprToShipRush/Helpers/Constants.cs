using System;
using System.Collections.Generic;
using System.Text;

namespace RepairShoprToShipRush.Helpers
{
    public static class Constants
    {
        public const string xmlPayloadTemplate = "<?xml version = \"1.0\"?><Request><ShipTransaction><Shipment><Package><PackageActualWeight>0</PackageActualWeight></Package><DeliveryAddress><Address><FirstName>{0}</FirstName><Company>{1}</Company><Address1>{2}</Address1><Address2>{3}</Address2><City>{4}</City><State>{5}</State><Country>US</Country><StateAsString>{5}</StateAsString><CountryAsString>US</CountryAsString><PostalCode>{6}</PostalCode><Phone>{7}</Phone><EMail>{8}</EMail></Address></DeliveryAddress></Shipment><Order>{9}<OrderNumber>{10}</OrderNumber><PaymentStatus>2</PaymentStatus><ItemsTax>{11}</ItemsTax><Total>{12}</Total><ItemsTotal>{13}</ItemsTotal><BillingAddress><FirstName>{0}</FirstName><Company>{1}</Company><Address1>{2}</Address1><Address2>{3}</Address2><City>{4}</City><State>{5}</State><Country>US</Country><StateAsString>{5}</StateAsString><CountryAsString>US</CountryAsString><PostalCode>{6}</PostalCode><Phone>{7}</Phone><EMail>{8}</EMail></BillingAddress><ShippingAddress><FirstName>{0}</FirstName><Company>{1}</Company><Address1>{2}</Address1><Address2>{3}</Address2><City>{4}</City><State>{5}</State><Country>US</Country><StateAsString>{5}</StateAsString><CountryAsString>US</CountryAsString><PostalCode>{6}</PostalCode><Phone>{7}</Phone><EMail>{8}</EMail></ShippingAddress></Order></ShipTransaction></Request>";
        public const string itemPayloadTemplate = "<ShipmentOrderItem><Name>{0}</Name><Price>{1}</Price><Quantity>{2}</Quantity><Total>{3}</Total></ShipmentOrderItem>";
    }
}
