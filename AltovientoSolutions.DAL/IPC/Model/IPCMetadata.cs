using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AltovientoSolutions.DAL.IPC.Model
{
    public class IPCMetadata
    {
        private LinkedList<NavigationPosition> navigation = new LinkedList<NavigationPosition>();

        public LinkedList<NavigationPosition> Navigation
        {
            get { return navigation; }
            set { navigation = value; }
        }


        private bool rTL = false;

        public bool RTL
        {
            get { return rTL; }
            set { rTL = value; }
        }


        public int NumberOfChapter { get; set; }
        public int NumberOfPages { get; set; }
        public int NumberOfEntries { get; set; }




    }
}
