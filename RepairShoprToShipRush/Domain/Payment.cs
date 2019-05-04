using System;
using System.Collections.Generic;
using System.Text;

namespace RepairShoprToShipRush.Domain
{
    public class Payment
    {
        public int? id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public bool? success { get; set; }
        public float payment_amount { get; set; }
        public List<int> invoice_ids { get; set; }
        public string ref_num { get; set; }
        public string applied_at { get; set; }
        public string payment_method { get; set; }
        public object transaction_response { get; set; }
    }
}
