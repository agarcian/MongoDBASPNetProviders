using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AltovientoSolutions.DAL.IPC.Model
{
    public class Page
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>
        /// The ID.
        /// </value>
        public string ID { get; set; }

        private List<Translation> title = new List<Translation>();

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title
        {
            get;
            set;
        }


        private List<Entry> entry = new List<Entry>();

        public List<Entry> Entry
        {
            get { return entry; }
            set { entry = value; }
        }


        public string IllustrationID { get; set; }

        private List<Callout> callout = new List<Callout>();

        public List<Callout> Callout
        {
            get { return callout; }
            set { callout = value; }
        }


    }
}
