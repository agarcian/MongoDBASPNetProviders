// Code downloaded from: http://www.codeproject.com/KB/aspnet/Website_URL_Screenshot.aspx

using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Reflection;

namespace AltovientoSolutions.Common.Util.Thumbnails
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class HandlerWSThumb : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            Bitmap thumb = null;
            string url = null;
            int bw = 800; //valor por defeito
            int bh = 600; //valor por defeito
            int tw = 0;    // sem thumbnail 
            int th = 0;

            context.Response.ContentType = "image/jpeg";
            if (context.Request["url"] != null)
            {
                if (context.Request["url"].ToString().ToLower().Contains("http://") || context.Request["url"].ToString().ToLower().Contains("https://"))
                    url = context.Request["url"].ToString();
                else
                    url = "http://" + context.Request["url"].ToString();
            }

            if (context.Request["bw"] != null)
                bw = Int32.Parse(context.Request["bw"].ToString());

            if (context.Request["bh"] != null)
                bh = Int32.Parse(context.Request["bh"].ToString());

            if (context.Request["tw"] != null)
                tw = Int32.Parse(context.Request["tw"].ToString());

            if (context.Request["th"] != null)
                th = Int32.Parse(context.Request["th"].ToString());

            // return context bitmap
            thumb = GetWebSiteThumbnail(url, bw, bh);

            if (tw != 0 && th != 0)
                thumb.GetThumbnailImage(tw, th, null, IntPtr.Zero).Save(context.Response.OutputStream, ImageFormat.Jpeg);
            else
                thumb.Save(context.Response.OutputStream, ImageFormat.Jpeg);

            thumb.Dispose();
        }

        public static Bitmap GetWebSiteThumbnail(string Url, int BrowserWidth, int BrowserHeight)
        {
            WebsiteThumbnailImage thumbnailGenerator = new WebsiteThumbnailImage(Url, BrowserWidth, BrowserHeight);
            return thumbnailGenerator.GenerateWebSiteThumbnailImage();
        }

        private class WebsiteThumbnailImage
        {
            public WebsiteThumbnailImage(string Url, int BrowserWidth, int BrowserHeight)
            {
                this.m_Url = Url;
                this.m_BrowserWidth = BrowserWidth;
                this.m_BrowserHeight = BrowserHeight;
            }

            private string m_Url = null;
            public string Url
            {
                get { return m_Url; }
                set { m_Url = value; }
            }

            private Bitmap m_Bitmap = null;
            public Bitmap ThumbnailImage
            {
                get { return m_Bitmap; }
            }

            private int m_BrowserWidth;
            public int BrowserWidth
            {
                get { return m_BrowserWidth; }
                set { m_BrowserWidth = value; }
            }

            private int m_BrowserHeight;
            public int BrowserHeight
            {
                get { return m_BrowserHeight; }
                set { m_BrowserHeight = value; }
            }

            public Bitmap GenerateWebSiteThumbnailImage()
            {
                Thread m_thread = new Thread(new ThreadStart(_GenerateWebSiteThumbnailImage));
                m_thread.SetApartmentState(ApartmentState.STA);
                m_thread.Start();
                m_thread.Join();
                return m_Bitmap;
            }

            private void _GenerateWebSiteThumbnailImage()
            {
                WebBrowser m_WebBrowser = new WebBrowser();
                m_WebBrowser.ScrollBarsEnabled = false;
                m_WebBrowser.Navigate(m_Url);
                m_WebBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(WebBrowser_DocumentCompleted);
                while (m_WebBrowser.ReadyState != WebBrowserReadyState.Complete)
                    Application.DoEvents();
                m_WebBrowser.Dispose();
            }

            private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
            {
                WebBrowser m_WebBrowser = (WebBrowser)sender;
                m_WebBrowser.ClientSize = new Size(this.m_BrowserWidth, this.m_BrowserHeight);
                m_WebBrowser.ScrollBarsEnabled = false;
                m_Bitmap = new Bitmap(m_WebBrowser.Bounds.Width, m_WebBrowser.Bounds.Height);
                m_WebBrowser.BringToFront();
                m_WebBrowser.DrawToBitmap(m_Bitmap, m_WebBrowser.Bounds);
                //m_Bitmap = (Bitmap)m_Bitmap.GetThumbnailImage(m_ThumbnailWidth, m_ThumbnailHeight, null, IntPtr.Zero);
            }
        }

        // Resusable flag
        public bool IsReusable
        {
            get { return false; }
        }
    }
}