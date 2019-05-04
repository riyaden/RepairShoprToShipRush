using System;
using System.Collections.Generic;
using System.Text;

namespace RepairShoprToShipRush.Domain
{
    public class Customer
    {
        public int? id { get; set; }
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
        public bool? get_sms { get; set; }
        public bool? opt_out { get; set; }
        public bool? disabled { get; set; }
        public bool? no_email { get; set; }
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
}
