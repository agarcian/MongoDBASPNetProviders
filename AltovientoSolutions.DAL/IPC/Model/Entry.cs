using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AltovientoSolutions.DAL.IPC.Model
{
    public class Entry
    {
        public string ID { get; set; }
        public int Sequence { get; set; }
        public string Position { get; set; }
        public string MaterialNumber { get; set; }
        public string Description { get; set; }
        public string Quantity { get; set; }
        public string UOM { get; set; }
        public string Comment { get; set; }
    }
}
