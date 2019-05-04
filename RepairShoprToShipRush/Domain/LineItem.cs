using System;
using System.Collections.Generic;
using System.Text;

namespace RepairShoprToShipRush.Domain
{
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
}
