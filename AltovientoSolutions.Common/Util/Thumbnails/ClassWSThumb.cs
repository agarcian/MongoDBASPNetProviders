// Code downloaded from: http://www.codeproject.com/KB/aspnet/Website_URL_Screenshot.aspx

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace AltovientoSolutions.Common.Util.Thumbnails
{
    public class ClassWSThumb
    {
        public static Bitmap GetWebSiteThumbnail(string Url, int BrowserWidth, int BrowserHeight, int ThumbnailWidth, int ThumbnailHeight)
        {
            return new WSThumb(Url, BrowserWidth, BrowserHeight, ThumbnailWidth, ThumbnailHeight).GetWSThumb();
        }

        private class WSThumb
        {
            public WSThumb(string Url, int BW, int BH, int TW, int TH)            {
                __Url = Url;
                __BrowserWidth = BW;
                __BrowserHeight = BH;
                __ThumbnailWidth = TW;
                __ThumbnailHeight = TH;
            }

            private Bitmap __Bitmap = null;            
            private string __Url = null;
            private int __ThumbnailWidth;
            private int __ThumbnailHeight;
            private int __BrowserWidth;
            private int __BrowserHeight;

            public string Url            {
                get{return __Url;}
                set{__Url = value;}
            }
            
            public Bitmap ThumbnailImage            {
                get{return __Bitmap;}
            }

            public int ThumbnailWidth            {
                get{return __ThumbnailWidth;}
                set{__ThumbnailWidth = value;}
            }

            public int ThumbnailHeight            {
                get {return __ThumbnailHeight;}
                set {__ThumbnailHeight = value;}
            }

            public int BrowserWidth            {
                get{return __BrowserWidth;}
                set{__BrowserWidth = value;}
            }

            public int BrowserHeight            {
                get{return __BrowserHeight;}
                set{__BrowserHeight = value;}
            }

            public Bitmap GetWSThumb()            {
                ThreadStart __threadStart = new ThreadStart(_GenerateWSThumb);
                Thread __thread = new Thread(__threadStart);

                __thread.SetApartmentState(ApartmentState.STA);
                __thread.Start();
                __thread.Join();
                return __Bitmap;
            }

            private void _GenerateWSThumb()            {
                WebBrowser __WebBrowser = new WebBrowser();
                __WebBrowser.ScrollBarsEnabled = false;
                __WebBrowser.Navigate(__Url);
                __WebBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(WebBrowser_DocumentCompleted);
                while (__WebBrowser.ReadyState != WebBrowserReadyState.Complete)
                    Application.DoEvents();
                __WebBrowser.Dispose();
            }

            private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)            {
                WebBrowser __WebBrowser = (WebBrowser)sender;
                __WebBrowser.ClientSize = new Size(this.__BrowserWidth, this.__BrowserHeight);
                __WebBrowser.ScrollBarsEnabled = false;
                __Bitmap = new Bitmap(__WebBrowser.Bounds.Width, __WebBrowser.Bounds.Height);
                __WebBrowser.BringToFront();
                __WebBrowser.DrawToBitmap(__Bitmap, __WebBrowser.Bounds);
                
                if (__ThumbnailHeight != 0 && __ThumbnailWidth !=0)
                __Bitmap = (Bitmap)__Bitmap.GetThumbnailImage(__ThumbnailWidth, __ThumbnailHeight, null, IntPtr.Zero);                
            }
        }
    }
}


