﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.Configuration;

namespace DiscountSystem
{
    public class CryptorEngine
    {


        //Получить количество дней от нулевой даты
        //
        public static string get_count_day()
        {
            string result = "";

            DateTime EraStart = new DateTime(1,1,1);

            int daysFromEraStart = (DateTime.Now - EraStart).Days;

            result = daysFromEraStart.ToString();

            return result;

        }

        public static string get_count_day_tsd()
        {
            string result = "";

            DateTime EraStart = new DateTime(1, 1, 1);
            int daysFromEraStart = (DateTime.Now - EraStart).Days;
            result = daysFromEraStart.ToString();
            for (int i = 0; i < 2; i++)
            {
                result += result;
            }
            
            return result;

        }

        public static string Encrypt(string toEncrypt, bool useHashing, string key)
        {
            byte[] keyArray;
            //System.IO.File.AppendAllText("C:\\DistrCashProgram\\Russia\\Test.txt", "1" + "\r\n");
            //System.IO.File.AppendAllText("C:\\DistrCashProgram\\Russia\\Test.txt", toEncrypt.Length.ToString() + "\r\n");
            //System.IO.File.AppendAllText("C:\\DistrCashProgram\\Russia\\Str.txt", toEncrypt);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            //System.IO.File.AppendAllText("C:\\DistrCashProgram\\Russia\\Test.txt", "2" + "\r\n");

            System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader();
            // Get the key from config file
            //string key = (string)settingsReader.GetValue("SecurityKey", typeof(String));
            //System.Windows.Forms.MessageBox.Show(key);
            if (useHashing)
            {
                //System.IO.File.AppendAllText("C:\\DistrCashProgram\\Russia\\Test.txt", "3" + "\r\n");
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
                //System.IO.File.AppendAllText("C:\\DistrCashProgram\\Russia\\Test.txt", "4" + "\r\n");
            }
            else
            {
                keyArray = UTF8Encoding.UTF8.GetBytes(key);                
            }
                        
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();            
            tdes.Key = keyArray;            
            tdes.Mode = CipherMode.ECB;            
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateEncryptor();            
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);            
            tdes.Clear();            
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        /// <summary>
        /// DeCrypt a string using dual encryption method. Return a DeCrypted clear string
        /// </summary>
        /// <param name="cipherString">encrypted string</param>
        /// <param name="useHashing">Did you use hashing to encrypt this data? pass true is yes</param>
        /// <returns></returns>
        public static string Decrypt(string cipherString, bool useHashing, string key)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(cipherString);

            System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader();
            //Get your key from config file to open the lock!
            //string key = (string)settingsReader.GetValue("SecurityKey", typeof(String));

            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
            }
            else
            {
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            tdes.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }
    }
}
