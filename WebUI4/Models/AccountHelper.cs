using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Security.Principal;
using System.Web.Profile;
using AltovientoSolutions.Security;
using AltovientoSolutions.Common.Util;

namespace WebUI4.Models
{
    public class AccountHelper
    {
        const string SEED = "xhdoiey3=dyuanbdkjad";


        public enum ProfileNameFormat
        {
            FirstNameOnly,
            FirstNameLastName,
            LastNameFirstName,
            Username
        }
            

        /// <summary>
        /// Gets the name of the user in the requested format.  Falls back to the username if the fields are not available.
        /// </summary>
        /// <param name="Format"></param>
        /// <returns>The name of the user in the required format. Empty string if not authenticated.</returns>
        public static string GetProfileName(ProfileNameFormat Format)
        {
            string profileName = String.Empty;

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return profileName;
            }
             
            
            ProfileCommon profile = (ProfileCommon)ProfileBase.Create(HttpContext.Current.User.Identity.Name);


            switch (Format)
            {


                case ProfileNameFormat.FirstNameOnly:
                    if (!String.IsNullOrEmpty(profile.FirstName))
                    {
                        profileName = profile.FirstName;
                    }
                    else
                    {
                        // falls back to the username when no available
                        profileName = HttpContext.Current.User.Identity.Name;
                    }

                    break;

                case ProfileNameFormat.FirstNameLastName:

                    profileName = String.Format("{0} {1}", profile.FirstName, profile.LastName);

                    if (String.IsNullOrEmpty(profileName.Trim()))
                    {
                        // falls back to the username when no available
                        profileName = HttpContext.Current.User.Identity.Name;
                    }


                    break;

                case ProfileNameFormat.LastNameFirstName:

                    profileName = String.Format("{1}, {0}", profile.FirstName, profile.LastName);

                    if (String.IsNullOrEmpty(profileName.Trim()))
                    {
                        // falls back to the username when no available
                        profileName = HttpContext.Current.User.Identity.Name;
                    }
                    break;


                case ProfileNameFormat.Username:

                    profileName = HttpContext.Current.User.Identity.Name;
                    break;

            }


            return profileName;
        }


        /// <summary>
        /// Gets the token for invitation.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public static string GetTokenForInvitation(string email)
        {
            if (String.IsNullOrEmpty(email))
                throw new ArgumentException("The email cannot be null");

            string token = Password.EncodeMessageWithPassword(String.Format("{0}#{1}", email, DateTime.Now), SEED);

            return token;
        }


        /// <summary>
        /// Gets the email from token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public static bool GetEmailFromToken(string token, out string email)
        {
            email = String.Empty;


            string message = Password.DecodeMessageWithPassword(token, SEED);
            string[] messageParts = message.Split('#');

            if (messageParts.Count() != 2)
            {
                return false;
                // the token was not generated correctly.
            }
            else
            {
                email = messageParts[0];
                return true;
            }
        }



        /// <summary>
        /// Helper function used to generate a token to be used in the message sent to users when registered the first time to confirm their email address.
        /// </summary>
        /// <param name="email">The email address to encode.</param>
        /// <returns>The token generated from the email address, timestamp, and SEED value.</returns>
        public static string GetTokenForValidation(string email)
        {
            if (String.IsNullOrEmpty(email))
                throw new ArgumentException("The email cannot be null");

            string token = Password.EncodeMessageWithPassword(String.Format("{0}#{1}", email, DateTime.Now), SEED);

            return token;
        }


        /// <summary>
        /// Validates whether a given token is valid for a determined email address.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <param name="email">The email address to use in the validation.</param>
        /// <returns><c>true</c> if the token is valid, <c>false</c> otherwise.</returns>
        public static bool IsTokenValid(string token, string email)
        {
            return IsTokenValid(token, email, DateTime.Now);
        }


        /// <summary>
        /// Core method to validate a token that also offers a timestamp for testing.  In production mode should always be DateTime.Now.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <param name="email">the email address to use in the validation.</param>
        /// <param name="timestamp">The timestamp representing the time in which the validation is performed.</param>
        /// <returns><c>true</c> if the token is valid, <c>false</c> otherwise.</returns>
        public static bool IsTokenValid(string token, string email, DateTime timestamp)
        {
            if (String.IsNullOrEmpty(token))
                throw new ArgumentException("The token cannot be null");

            try
            {
                string message = Password.DecodeMessageWithPassword(token, SEED);
                string[] messageParts = message.Split('#');

                if (messageParts.Count() != 2)
                {
                    return false;
                    // the token was not generated correctly.
                }
                else
                {
                    string messageEmail = messageParts[0];
                    string messageDate = messageParts[1];

                    // If the emails are the same and the date in which the token was created is no longer than 5 days, then it is valid. Otherwise, it is not. 
                    return (String.Compare(email, messageEmail, true) == 0 && timestamp.Subtract(DateTime.Parse(messageDate)).Days < 5);
                }
            }
            catch (Exception)
            {
                // could not decrypt the message. The token has been tampered with.
                return false;
            }
        }

    }
}