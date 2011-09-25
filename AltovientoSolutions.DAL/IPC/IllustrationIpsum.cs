using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace AltovientoSolutions.DAL.IPC
{

    public class Callout
    {
        public int X0 { get; set; }

        public int X1 { get; set; }

        public int Y0 { get; set; }

        public int Y1 { get; set; }

        public string Text { get; set; }

        public string ID { get; set; }

    }



    public class IllustrationIpsum
    {

        public static Bitmap GenerateRandomImage(int width, int height, int numberOfCallouts, bool allowRepeatedEntries, out  List<Callout> callouts )
        {
            Random rnd = new Random(DateTime.Now.Millisecond);

            callouts = new List<Callout>();

            Bitmap bitmap = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bitmap);

            Brush oBrush = new SolidBrush(Color.Gray);
            
            g.DrawClosedCurve(new Pen(oBrush), new Point[] {
                new Point(rnd.Next(width/2), rnd.Next(height/2)),
                new Point(rnd.Next(width/2), rnd.Next(height/2)),
                new Point(rnd.Next(width/2) + rnd.Next(width/2), rnd.Next(height/2) + rnd.Next(height/2)),
                new Point(rnd.Next(width/2) + rnd.Next(width/2), rnd.Next(height/2) + rnd.Next(height/2)),
                new Point(rnd.Next(width/2) + rnd.Next(width/2), rnd.Next(height/2) + rnd.Next(height/2)),
                new Point(rnd.Next(width/2), rnd.Next(height/2))    
            });

            g.DrawClosedCurve(new Pen(oBrush), new Point[] {
                new Point(rnd.Next(width), rnd.Next(height)),
                new Point(rnd.Next(width) + rnd.Next(width), rnd.Next(height) + rnd.Next(height)),
                new Point(rnd.Next(width), rnd.Next(height)),
                new Point(rnd.Next(width) + rnd.Next(width), rnd.Next(height) + rnd.Next(height)),
                new Point(rnd.Next(width), rnd.Next(height))    
            });

            g.DrawString("IllustratedPartsCatalogs.org", new Font(FontFamily.GenericSerif, 14, FontStyle.Regular), new SolidBrush(Color.LightGray), new Point(40, height - 60));
  
            // Draw the call outs.

            for (int i = 0; i < numberOfCallouts; i++)
            {
                Point point =  new Point(rnd.Next(width), rnd.Next(height));
                g.DrawString(i.ToString(), new Font(FontFamily.GenericSerif, 12, FontStyle.Regular), new SolidBrush(Color.Navy), point);
                callouts.Add(new Callout() { ID = ("x_" + rnd.Next(10000)).ToString(), X0 = point.X - 15, X1 = point.X + 15, Y0 = point.Y - 15, Y1 = point.Y + 15, Text = i.ToString() });
            }

            return bitmap;
        }
    }
}
