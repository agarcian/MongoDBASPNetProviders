using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Data;
using System.Resources;

namespace AltovientoSolutions.Common.Util
{
	/// <summary>
	/// Implements some functions to support password manipulation or generation
	/// </summary>
	public class Password
	{
		/// <summary>
		/// Takes a string and generates a hash value of 16 bytes.
		/// </summary>
		/// <param name="str">The string to be hashed</param>
		/// <param name="passwordFormat">Selects the hashing algorithm used. Accepted values are "sha1" and "md5".</param>
		/// <returns>A hex string of the hashed password.</returns>
		public static string EncodeString(string str, string passwordFormat)
		{
			if (str == null)
				return null;
			
			ASCIIEncoding AE = new ASCIIEncoding();
			byte[] result;
			switch (passwordFormat)
			{
				case "sha1":                    
					SHA1 sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
					result = sha1.ComputeHash(AE.GetBytes(str));
					break;
				case "md5":
					MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
					result = md5.ComputeHash(AE.GetBytes(str));
					break;
				default:
					throw new ArgumentException("Invalid format value. Accepted values are 'sha1' and 'md5'.", "passwordFormat");
			}

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            StringBuilder sb = new StringBuilder(16);
            for (int i = 0; i < result.Length; i++)
            {
                sb.Append(result[i].ToString("x2"));
            }


			return sb.ToString();
		}

		/// <summary>
		/// Takes a string and generates a hash value of 16 bytes.  Uses "md5" by default.
		/// </summary>
		/// <param name="str">The string to be hashed</param>
		/// <returns>A hex string of the hashed password.</returns>
		public static string EncodeString(string str)
		{
			return EncodeString(str, "md5");
		}



        /// <summary>
        /// Takes a string and generates a hash value of 16 bytes.
        /// </summary>
        /// <param name="str">The string to be hashed</param>
        /// <param name="passwordFormat">Selects the hashing algorithm used. Accepted values are "sha1" and "md5".</param>
        /// <returns>A string of the hashed password.</returns>
        public static string EncodeBinary(byte[] buffer, string passwordFormat)
        {
            if (buffer == null)
                return null;

            byte[] result;
            switch (passwordFormat)
            {
                case "sha1":
                    SHA1 sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                    result = sha1.ComputeHash(buffer);
                    break;
                case "md5":
                    MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    result = md5.ComputeHash(buffer);
                    break;
                default:
                    throw new ArgumentException("Invalid format value. Accepted values are 'sha1' and 'md5'.", "passwordFormat");
            }


            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            StringBuilder sb = new StringBuilder(16);
            for (int i = 0; i < result.Length; i++)
            {
                sb.Append(result[i].ToString("x2"));
            }


            return sb.ToString();
        }

        /// <summary>
        /// Encodes the buffer using the default cryptographic provider.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public static string EncodeBinary(byte[] buffer)
        {
            return EncodeBinary(buffer, "md5");
        }





		/// <summary>
		/// Creates a random alphanumeric password.
		/// </summary>
		/// <returns>A default lenght character string with the new password.</returns>
		/// <remarks>The default lenght of the password is eight (8) characters.</remarks>
		public static string CreateRandomPassword()
		{
			//Default lenght is 8 characters
			return CreateRandomPassword(8);
		}

		/// <summary>
		/// Creates a random alphanumeric password on dimension (Lenght).
		/// </summary>
		/// <param name="Lenght">The number of characters in the password</param>
		/// <returns>The generated password</returns>
		public static string CreateRandomPassword(int Lenght)
		{
			Random rnd = new Random(Convert.ToInt32(DateTime.Now.Millisecond));  //Creates the seed from the time
			string Password="";
			while (Password.Length < Lenght ) 
			{
				char newChar = Convert.ToChar((int)((122 - 48 + 1) * rnd.NextDouble() + 48));
				if ((((int) newChar) >= ((int) 'A')) & (((int) newChar) <= ((int) 'Z')) | (((int) newChar) >= ((int) 'a')) & (((int) newChar) <= ((int) 'z')) | (((int) newChar) >= ((int) '0')) & (((int) newChar) <= ((int) '9')))
					Password += newChar;
			}
			return Password;
		}
	
		/// <summary>
		/// Takes a text message and encrypts it using a password as a key.
		/// </summary>
		/// <param name="plainMessage">A text to encrypt.</param>
		/// <param name="password">The password to encrypt the message with.</param>
		/// <returns>Encrypted string.</returns>
		/// <remarks>This method uses TripleDES symmmectric encryption.</remarks>
		public static string EncodeMessageWithPassword(string plainMessage, string password)
		{
			if (plainMessage == null)
				throw new ArgumentNullException("encryptedMessage", "The message cannot be null");
			
			TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
			des.IV = new byte[8];
			
			//Creates the key based on the password and stores it in a byte array.
			PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[0]);
			des.Key = pdb.CryptDeriveKey("RC2", "MD5", 128, new byte[8]);

			MemoryStream ms = new MemoryStream(plainMessage.Length * 2);
			CryptoStream encStream = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
			byte[] plainBytes = Encoding.UTF8.GetBytes(plainMessage);
			encStream.Write(plainBytes, 0, plainBytes.Length);
			encStream.FlushFinalBlock();
			byte[] encryptedBytes = new byte[ms.Length];
			ms.Position = 0;
			ms.Read(encryptedBytes, 0, (int)ms.Length);
			encStream.Close();

			return Convert.ToBase64String(encryptedBytes);
		}

		/// <summary>
		/// Takes an encrypted message using TripleDES and a password as a key and converts it to the original text message.
		/// </summary>
		/// <param name="encryptedMessage">The encrypted message to decode.</param>
		/// <param name="password">The password to decode the message.</param>
		/// <returns>The Decrypted message</returns>
		/// <remarks>This method uses TripleDES symmmectric encryption.</remarks>
		public static string DecodeMessageWithPassword(string encryptedMessage, string password)
		{
			if (encryptedMessage == null)
				throw new ArgumentNullException("encryptedMessage", "The encrypted message cannot be null");
			
			TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
			des.IV = new byte[8];
			
			//Creates the key based on the password and stores it in a byte array.
			PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[0]);
			des.Key = pdb.CryptDeriveKey("RC2", "MD5", 128, new byte[8]);
			
			//This line protects the + signs that get replaced by spaces when the parameter is not urlencoded when sent.
			encryptedMessage = encryptedMessage.Replace(" ", "+");
			MemoryStream ms = new MemoryStream(encryptedMessage.Length * 2);
			CryptoStream decStream = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
			
			byte[] plainBytes; 
			try 
			{
				byte[] encBytes = Convert.FromBase64String(Convert.ToString(encryptedMessage));
				decStream.Write(encBytes, 0, encBytes.Length);
				decStream.FlushFinalBlock();				
				plainBytes = new byte[ms.Length];
				ms.Position = 0;				
				ms.Read(plainBytes, 0, (int)ms.Length);
				decStream.Close();
			}
			catch(CryptographicException e)
			{
				throw new ApplicationException("Cannot decrypt message.  Possibly, the password is wrong", e);
			}

			return Encoding.UTF8.GetString(plainBytes);
		}
	}
}

