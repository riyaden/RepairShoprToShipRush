using System;
using System.Collections.Generic;
using System.Text;

namespace RepairShoprToShipRush.Domain
{
    public class Invoice
    {
        public int? id { get; set; }
        public string number { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string date { get; set; }
        public string due_date { get; set; }
        public string subtotal { get; set; }
        public string total { get; set; }
        public string tax { get; set; }
        public bool? verified_paid { get; set; }
        public bool? tech_marked_paid { get; set; }
        public int? ticket_id { get; set; }
        public object pdf_url { get; set; }
        public string balance_due { get; set; }
        public string note { get; set; }
        public List<LineItem> line_items { get; set; }
        public bool? is_paid { get; set; }
        public object location_id { get; set; }
        public object contact_id { get; set; }
        public string po_number { get; set; }
        public List<Payment> payments { get; set; }
        public string hardwarecost { get; set; }
        public Customer customer { get; set; }
        public int? customer_id { get; set; }
        public string customer_business_then_name { get; set; }
    }
}
