using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AltovientoSolutions.Common.Util
{

    public partial class Gravatar 
    {
        /// <summary>
        /// Rating Property
        /// </summary>
        public enum GravatarRating
        {
            g, pg, r, x
        }

        /// <summary>
        /// Icon Set Property
        /// </summary>
        public enum GravatarDefaultIcon
        {
            /// <summary>
            /// do not load any image if none is associated with the email hash, instead return an HTTP 404 (File Not Found) response.
            /// </summary>
            _404,
            /// <summary>
            /// (mystery-man) a simple, cartoon-style silhouetted outline of a person (does not vary by email hash)
            /// </summary>
            mm,
            /// <summary>
            /// A geometric pattern based on an email hash
            /// </summary>
            identicon, 
            /// <summary>
            /// A generated 'monster' with different colors, faces, etc.
            /// </summary>
            monsterid, 
            /// <summary>
            /// Generated faces with differing features and backgrounds
            /// </su    mmary>
            wavatar,
            /// <summary>
            /// Awesome generated, 8-bit arcade-style pixelated faces
            /// </summary>
            retro
        }

        /// <summary>
        /// Base URL for the Gravatar image
        /// </summary>
        private static string BaseURL = "http://www.gravatar.com/avatar/{0}?d={1}&s={2}&r={3}";
        private static string BaseURLWithDefaultImage = "http://www.gravatar.com/avatar/{0}?d={1}&s={2}&r={3}&d={4}";


        //#region Properties
        
        ///// <summary>
        ///// Gets or sets the Email.
        ///// </summary>
        //public string Email {get; set;}

        
        ///// <summary>
        ///// Gets or sets the Rating.
        ///// </summary>
        //public GravatarRating Rating;

        ///// <summary>
        ///// Gets or sets the DefaultIcon.
        ///// </summary>
        //public GravatarDefaultIcon DefaultIcon { get; set; }

        ///// <summary>
        ///// Gets or sets the Size.
        ///// </summary>
        //public System.Drawing.Size Size { get; set; }
        //#endregion

        /// <summary>
        /// Small MD5 Function
        /// </summary>
        /// <param name="Email"></param>
        /// <returns>Hash of the email address passed.</returns>
        public static string MD5(string Email)
        {
            if (Email == null)
                Email = "unknown@example.com";


            System.Security.Cryptography.MD5CryptoServiceProvider md5Obj =
                new System.Security.Cryptography.MD5CryptoServiceProvider();

            byte[] bytesToHash = System.Text.Encoding.ASCII.GetBytes(Email);

            bytesToHash = md5Obj.ComputeHash(bytesToHash);

            string strResult = String.Empty;

            foreach (byte b in bytesToHash)
            {
                strResult += b.ToString("x2");
            }

            return strResult;
        }

        public static string GetUrl(string Email)
        {
            int Size = 80;  // the default in Gravatar.
            return GetUrl(Email, Size);
        }

        public static string GetUrl(string Email, int Size)
        {
            // Defaults to the mistery man.
            return GetUrl(Email, Size, GravatarDefaultIcon.mm);
        }

        public static string GetUrl(string Email, int Size, GravatarDefaultIcon DefaultIcon)
        {
            return GetUrl(Email, Size, DefaultIcon, GravatarRating.g);
        }

        public static string GetUrl(string Email, int Size, GravatarDefaultIcon DefaultIcon, GravatarRating Rating)
        {
            string hashedEmail = MD5(Email);
            return string.Format(BaseURL, hashedEmail, DefaultIcon, Size, Rating);
        }

        /// <summary>
        /// Gets the Gravatar URL and allows for the specification of a url for the default image.
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="Size"></param>
        /// <param name="Rating">The Gravatar maximum rating.</param>
        /// <param name="DefaultImage">URL of the default image.  Do not URL Encode as that is handled in the implementation.</param>
        /// <returns></returns>
        public static string GetUrl(string Email, int Size, GravatarRating Rating, string DefaultImage)
        {
            string hashedEmail = MD5(Email);
            return string.Format(BaseURLWithDefaultImage, hashedEmail, Size, Rating, HttpUtility.UrlEncode(DefaultImage));
        }
    }
}
