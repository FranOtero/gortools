using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Scheduler.GTools.Models
{
    public class MaterialOrder
    {
        public string Reference { get; set; }
        public int Quantity { get; set; }
        public DateTime Target { get; set; }
    }
}
