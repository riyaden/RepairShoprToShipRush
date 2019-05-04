using System;
using System.Collections.Generic;
using System.Text;

namespace RepairShoprToShipRush.Domain
{
    public class InvoicesList
    {
        public List<Invoice> invoices { get; set; }
        public Meta meta { get; set; }
    }
}
