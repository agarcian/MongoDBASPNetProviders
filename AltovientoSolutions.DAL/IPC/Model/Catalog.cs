using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AltovientoSolutions.DAL.IPC.Model
{
    public class Catalog
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>
        /// The ID.
        /// </value>
        public string ID { get; set; }

        public string Title { get; set; }

        public string LanguageCode { get; set; }

        private List<string> franchise = new List<string>();

        /// <summary>
        /// Gets or sets the brand.
        /// </summary>
        /// <value>
        /// The brand.
        /// </value>
        public List<string> Franchise
        {
            get { return franchise; }
            set { franchise = value; }
        }

        private List<Chapter> chapter = new List<Chapter>();

        /// <summary>
        /// Gets or sets the chapter.
        /// </summary>
        /// <value>
        /// The chapter.
        /// </value>
        public List<Chapter> Chapter
        {
            get { return chapter; }
            set { chapter = value; }
        }


        private IPCMetadata ipcMetadata = new IPCMetadata();

        public IPCMetadata IpcMetadata
        {
            get { return ipcMetadata; }
            set { ipcMetadata = value; }
        }


    }
}
