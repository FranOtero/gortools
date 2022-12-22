using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Scheduler.GTools.Models
{
    public class Component
    {
        public string Parent { get; set; }
        public string SubComponent { get; set; }
        public int Quantity { get; set; }
        public int Recipe { get; set; }
    }
}
