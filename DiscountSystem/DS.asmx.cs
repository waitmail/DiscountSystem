﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
//using System.Web;
using System.Web.Services;
//using System.Web.Services.Protocols;
using System.Data.SqlClient;
//using System.Data.SqlTypes;
using System.Text;
//using System.Text.RegularExpressions;
//using System.Data.SqlServerCe;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO.Compression;


namespace DiscountSystem
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class DS : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }
        
        [WebMethod]
        public bool ServiceIsWorker()
        {
            return true;
        }

        [WebMethod]
        public string[] GetTypesCard()
        {
            SqlConnection conn = null; string[] result = new string[1]; result[0] = "-1";
            conn = new SqlConnection(getConnectionString());            
            try
            {
                StringBuilder result_query = new StringBuilder();
                conn.Open();
                string query = "SELECT code,sum,percent,name  FROM types_card";
                SqlCommand command = new SqlCommand(query, conn);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result_query.Append("|'" + reader[0].ToString() + "','" + reader[1].ToString().Replace(",", ".") + "'," + reader[2].ToString().Replace(",", ".") + ",'" +
                        reader[3].ToString()+"'");
                }
                reader.Close();
                conn.Close();
                result = result_query.ToString().Substring(1,result_query.ToString().Length-1).Split('|');
                
            }
            catch
            {


            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            
            return result;            
        }

        //[WebMethod]
        //public bool BonusClientWrittenOff(string nick_shop, string data)
        //{
        //      bool result = true;
        //    //Расшифровка пакета, затем проверка на валидность и если все хорошо то чтение данных
        //    //по коду магазина пытаемся получить часть ключа
        //    string code_shop = get_id_database(nick_shop);
        //    if (code_shop.Trim().Length == 0)
        //    {
        //        result = false;
        //        return result;
        //    }
        //    string count_day = CryptorEngine.get_count_day();
        //    string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();
        //    string[] decrypt_data = CryptorEngine.Decrypt(data, true, key).Split('*');
        //    //с пакетом какие то проблемы
        //    if ((decrypt_data[0].Trim() != nick_shop.Trim()) || (decrypt_data[decrypt_data.Length - 1].Trim() != code_shop.Trim()))
        //    {
        //        result = false;
        //        return result;
        //    }            
        //    //Данные получены и расшифрованы
        //    //попробуем внести их в базу
        //    //nick_shop,client,bonuses_it_is_written_off,cash_desk_number,num_doc,code_shop
        //    SqlConnection conn = new SqlConnection(getConnectionString());
        //    SqlTransaction trans = null;
        //    //SqlTransaction trans = null;
        //    try
        //    {
        //        //string[] str=
        //        conn.Open();
        //        trans = conn.BeginTransaction();
        //        //string query = " SELECT COUNT(*) FROM temp_bonus_written_off WHERE code_client='"+decrypt_data[1]+"'"; //Проверка 
        //        string query = " SELECT SUM(first_query.bonuses_it_is_added - first_query.bonuses_it_is_written_off) AS TotalBonus FROM( " +
        //           " SELECT [sum] AS bonuses_it_is_added,0 as bonuses_it_is_written_off " +
        //           " FROM [dbo].[clients] " +
        //           " Where code='" + decrypt_data[1] + "'" +
        //           " UNION ALL " +
        //           " SELECT 0,bonuses_it_is_written_off " +
        //           " FROM [dbo].[temp_bonus_written_off] " +
        //           " Where code_client='" + decrypt_data[1] + "') AS first_query ";
        //        SqlCommand command = new SqlCommand(query, conn);
        //        command.Transaction = trans;
        //        Int64 rezult_query  = Convert.ToInt64(command.ExecuteScalar());
        //        if (rezult_query >= Convert.ToInt64(decrypt_data[2]))
        //        {
        //            query = " INSERT INTO temp_bonus_written_off(" +
        //            "shop," +
        //            "num_doc," +
        //            "num_cash," +
        //            "bonuses_it_is_written_off," +
        //            "code_client," +
        //            "date)VALUES('" +
        //            decrypt_data[0] + "'," +
        //            decrypt_data[4] + "," +
        //            decrypt_data[3] + "," +
        //            decrypt_data[2] + ",'" +
        //            decrypt_data[1] + "','" +
        //            DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + "')";
        //            command = new SqlCommand(query, conn);
        //            command.Transaction = trans;
        //            command.ExecuteNonQuery();
        //        }
        //        else
        //        {
        //            result = false;
        //        }
        //        trans.Commit();
        //        conn.Close();
        //        command.Dispose();
        //        trans.Dispose();
        //    }
        //    catch (SqlException)
        //    {
        //        if (trans != null)
        //        {
        //            trans.Rollback();
        //        }
        //        result = false;
        //    }
        //    finally
        //    {
        //        if (conn.State == ConnectionState.Open)
        //        {
        //            conn.Close();
        //        }
        //    }
        //    return result;
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="nick_shop"></param>
        /// <param name="data"></param>
        /// <returns></returns>

        [WebMethod]
        public string GetUsers(string nick_shop, string data, string scheme)
        {
            string result = "";

            //SqlConnection conn = null;
            //conn = new SqlConnection(getConnectionString());
            SqlConnection conn = new SqlConnection(scheme == "1" ? getConnectionString() : getConnectionString2());

            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                return result;
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();

            string[] decrypt_data = CryptorEngine.Decrypt(data, true, key).Split('|');


            // Неожиданная строка 
            if (decrypt_data.Length != 2)
            {
                return result;
            }

            //result = CryptorEngine.Encrypt(nick_shop.Trim() + "*" + result.Trim() + "*" + code_shop.Trim(), true, key);

            //с пакетом какие то проблемы
            if ((decrypt_data[0].Trim() != nick_shop.Trim()) || (decrypt_data[1].Trim() != code_shop.Trim()))
            {
                return result;
            }
            try
            {
                conn.Open();
                //string query = " SELECT code,name,rights,shop,password_m,password_b  FROM users WHERE shop='" + nick_shop +"' OR shop=''";
                string query = " SELECT users.code,users.name,users.rights,users_shops.shop,users.password_m,users.password_b,users.INN FROM users LEFT JOIN users_shops ON " +
                    "   users.code=users_shops.code WHERE users_shops.shop='" + nick_shop + "'  OR users_shops.shop=''";

                SqlCommand command = new SqlCommand(query, conn);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result += reader["code"].ToString() + "|" + 
                        reader["name"].ToString() + "|" + 
                        reader["rights"].ToString() + "|" + 
                        reader["shop"].ToString() + "|" + 
                        reader["password_m"].ToString()+ "|"+
                        reader["password_b"].ToString() + "|" +
                        (reader["INN"].ToString().Trim()=="" ? " " : reader["INN"].ToString().Trim())+ "||";
                        //reader["INN"].ToString().Trim()+ "||";
                }              
            }
            catch
            {

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            if (result != "")
            {
                result = result.Substring(0, result.Length - 2);                
                result = CryptorEngine.Encrypt(result, true, key);
            }
            
            return result;
        }

        ///// <summary>
        ///// передается вначале строки код магазина 
        ///// завершает пакет гуид магазина все разделено знаком * 
        ///// 
        ///// </summary>
        ///// <param name="num_shop"></param>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //[WebMethod]
        //public string GetFirstBuyClient(string nick_shop, string data)
        //{
        //    string result = "-1";

        //    //Расшифровка пакета, затем проверка на валидность и если все хорошо то чтение данных
        //    //по коду магазина пытаемся получить часть ключа

        //    string code_shop = get_id_database(nick_shop);
        //    if (code_shop.Trim().Length == 0)
        //    {
        //        return result;
        //    }

        //    string count_day = CryptorEngine.get_count_day();
        //    string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();

        //    string[] decrypt_data = CryptorEngine.Decrypt(data, true, key).Split('|');


        //    // Неожиданная строка 
        //    if (decrypt_data.Length != 3)
        //    {
        //        return result;
        //    }

        //    //result = CryptorEngine.Encrypt(nick_shop.Trim() + "*" + result.Trim() + "*" + code_shop.Trim(), true, key);


        //    //с пакетом какие то проблемы
        //    if ((decrypt_data[0].Trim() != nick_shop.Trim()) || (decrypt_data[2].Trim() != code_shop.Trim()))
        //    {
        //        return result;
        //    }

        //    string client = decrypt_data[1].Trim();

        //    SqlConnection conn = null;
        //    conn = new SqlConnection(getConnectionString());
        //    try
        //    {
        //        conn.Open();
        //        //string query = "SELECT SUM(bonus_counted) AS plus,SUM(bonus_writen_off) AS minus FROM sales_header WHERE [client] ='" + client + "'";
        //        string query = "SELECT SUM(bonus_counted) FROM sales_header WHERE client ='" + client + "' AND its_deleted=0";
        //        SqlCommand command = new SqlCommand(query, conn);
        //        object rez = command.ExecuteScalar();
        //        conn.Close();
        //        command.Dispose();
        //        if (rez != null)
        //        {
        //            result = rez.ToString();
        //        }
        //        else
        //        {
        //            result = "0";
        //        }
        //    }
        //    catch//Исключения пока никак не обрабатываем
        //    {

        //    }
        //    finally
        //    {
        //        if (conn.State == ConnectionState.Open)
        //        {
        //            conn.Close();
        //        }
        //    }

        //    //Зашифровать ответ

        //    result = CryptorEngine.Encrypt(nick_shop.Trim() + "*" + result.Trim() + "*" + code_shop.Trim(), true, key);

        //    return result;
        //}


        ///// <summary>
        ///// передается вначале строки код магазина 
        ///// завершает пакет гуид магазина все разделено знаком * 
        ///// 
        ///// </summary>
        ///// <param name="num_shop"></param>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //[WebMethod]
        //public string GetBonusOnClient_NEW(string nick_shop, string data)
        //{
        //    string result = "-1";

        //    //Расшифровка пакета, затем проверка на валидность и если все хорошо то чтение данных
        //    //по коду магазина пытаемся получить часть ключа
        //    string code_shop = get_id_database(nick_shop);
        //    if (code_shop.Trim().Length == 0)
        //    {
        //        return result;
        //    }
        //    string count_day = CryptorEngine.get_count_day();
        //    string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();
        //    string[] decrypt_data = CryptorEngine.Decrypt(data, true, key).Split('|');
        //    // Неожиданная строка 
        //    if (decrypt_data.Length != 3)
        //    {
        //        return result;
        //    }
        //    //result = CryptorEngine.Encrypt(nick_shop.Trim() + "*" + result.Trim() + "*" + code_shop.Trim(), true, key);
        //    //с пакетом какие то проблемы
        //    if ((decrypt_data[0].Trim() != nick_shop.Trim()) || (decrypt_data[2].Trim() != code_shop.Trim()))
        //    {
        //        return result;
        //    }

        //    string client = decrypt_data[1].Trim();

        //    SqlConnection conn = null;
        //    conn = new SqlConnection(getConnectionString());
        //    try
        //    {
        //        conn.Open();
        //        //string query = "SELECT sum FROM clients WHERE code ='" + client + "'";
        //        string query =" SELECT SUM(first_query.bonuses_it_is_added - first_query.bonuses_it_is_written_off) FROM( "+
        //            " SELECT [sum] AS bonuses_it_is_added,0 as bonuses_it_is_written_off "+
        //            " FROM [dbo].[clients] "+
        //            " Where code='"+client+"'"+
        //            " UNION ALL "+
        //            " SELECT 0,bonuses_it_is_written_off "+
        //            " FROM [dbo].[temp_bonus_written_off] "+
        //            " Where code_client='"+client+"') AS first_query ";
        //        SqlCommand command = new SqlCommand(query, conn);
        //        object rez = command.ExecuteScalar();
        //        conn.Close();
        //        command.Dispose();
        //        if (rez != null)
        //        {
        //            Int64 result_query = Convert.ToInt64(rez);
        //            if (result_query < 0)
        //            {
        //                result_query = 0;
        //            }
        //            result = result_query.ToString();
        //        }
        //        else
        //        {
        //            result = "0";
        //        }
        //    }
        //    catch//Исключения пока никак не обрабатываем
        //    {

        //    }
        //    finally
        //    {
        //        if (conn.State == ConnectionState.Open)
        //        {
        //            conn.Close();
        //        }
        //    }
        //    //Зашифровать ответ
        //    result = CryptorEngine.Encrypt(nick_shop.Trim() + "*" + result.Trim() + "*" + code_shop.Trim(), true, key);
        //    return result;
        //}


        // [WebMethod]
        //public string GetDiscountClientsV8DateTime(string nick_shop, string data)
        //{
        //    SqlConnection conn = null; string result = "-1";

        //    //if (!check_avalible_dataV8())
        //    //{
        //    //    return result;
        //    //}

        //    StringBuilder sb = new StringBuilder();

        //    string code_shop = get_id_database(nick_shop);
        //    if (code_shop.Trim().Length == 0)
        //    {
        //        return result;
        //    }

        //    string count_day = CryptorEngine.get_count_day();
        //    string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();
        //    string[] delimiters = new string[] { "|" };
        //    string decrypt_data = CryptorEngine.Decrypt(data.ToString(), true, key);
        //    string[] d = decrypt_data.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        //    if (d.Length != 3)
        //    {
        //        return result;
        //    }

        //    if ((d[0].Trim() != nick_shop) || (d[2].Trim() != code_shop))
        //    {
        //        return result;
        //    }

        //    DateTime datetime = new DateTime(Convert.ToInt64(d[1]));

        //    conn = new SqlConnection(getConnectionString());
        //    try
        //    {
        //        StringBuilder result_query = new StringBuilder();
        //        conn.Open();
        //        //string query = "SELECT code,name,sum,birthday,type_card,its_work  FROM clients WHERE datetime_update > '" + datetime.AddDays(-1).ToString("dd-MM-yyyy HH:mm:ss") + "'";
        //        string query = " SELECT TOP 1000 code,name,sum,birthday,type_card,its_work,datetime_update,phone,attribute  FROM clients WHERE datetime_update >= '" + datetime.ToString("dd-MM-yyyy HH:mm:ss") + "' order by datetime_update ";
        //        SqlCommand command = new SqlCommand(query, conn);
        //        SqlDataReader reader = command.ExecuteReader();
        //        while (reader.Read())
        //        {
        //           result_query.Append("|'" + reader[0].ToString() + "','" + reader[1].ToString().Replace(",", "") + "'," +
        //                reader[2].ToString().Replace(",", ".") + ",'" + reader.GetDateTime(3).ToString("yyyy-MM-dd") + "'," +
        //                reader[4].ToString() + "," + reader[5].ToString() + ",'" + reader.GetDateTime(6).ToString("yyyy-MM-dd HH:mm:ss") + "','" +
        //                (reader[7].ToString().Trim() == "" ? "0" : reader[7].ToString())+ "','" +
        //                reader[8].ToString().Trim()+"'");
        //        }
        //        reader.Close();
        //        conn.Close();
                               
        //        result = CryptorEngine.Encrypt(result_query.ToString(), true, key);
        //    }
        //    catch(Exception ex)
        //    {
        //        //result = ex.Message;
        //    }
        //    finally
        //    {
        //        if (conn.State == ConnectionState.Open)
        //        {
        //            conn.Close();
        //        }
        //    }        

        //    return result;
        //}

        #region GetBonusCardsV8DateTime  

        public class BonusCards
        {
            public List<BonusCard> ListBonusCards { get; set; }
            public string LocalLastDateDownloadBonusCards { get; set; }
        }


        public class BonusCard
        {
            public string code { get; set; }
            public string pin  { get; set; }

        }

        //[WebMethod]
        //public byte[] GetBonusCardsV8DateTime(string nick_shop, string data)
        //{
        //    SqlConnection conn = null; //string result = "-1";
        //    Byte[] result = Encoding.UTF8.GetBytes("-1");
        //    StringBuilder sb = new StringBuilder();

        //    string code_shop = get_id_database(nick_shop, scheme);
        //    if (code_shop.Trim().Length == 0)
        //    {
        //        return result;
        //    }

        //    string count_day = CryptorEngine.get_count_day();
        //    string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();
        //    string[] delimiters = new string[] { "|" };
        //    string decrypt_data = CryptorEngine.Decrypt(data.ToString(), true, key);
        //    string[] d = decrypt_data.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        //    if (d.Length != 3)
        //    {
        //        return result;
        //    }

        //    if ((d[0].Trim() != nick_shop) || (d[2].Trim() != code_shop))
        //    {
        //        return result;
        //    }

        //    DateTime datetime = new DateTime(Convert.ToInt64(d[1]));

        //    conn = new SqlConnection(getConnectionString());
        //    try
        //    {
        //        StringBuilder result_query = new StringBuilder();
        //        conn.Open();                
        //        string query = " SELECT TOP 1000 number,pin,datetime_update FROM bonus_cards WHERE datetime_update >= '" + datetime.ToString("dd-MM-yyyy HH:mm:ss") + "' order by datetime_update ";
        //        SqlCommand command = new SqlCommand(query, conn);
        //        SqlDataReader reader = command.ExecuteReader();
        //        BonusCards bonusCards = new BonusCards();
        //        bonusCards.ListBonusCards = new List<BonusCard>();
        //        while (reader.Read())
        //        {
        //            BonusCard bonusCard = new BonusCard();
        //            bonusCard.code = reader["number"].ToString();
        //            bonusCard.pin = reader["pin"].ToString();
        //            bonusCards.ListBonusCards.Add(bonusCard);
        //            bonusCards.LocalLastDateDownloadBonusCards = Convert.ToDateTime(reader["datetime_update"]).ToString("dd-MM-yyyy HH:mm:ss");
        //        }
        //        reader.Close();
        //        conn.Close();

        //        string jason = JsonConvert.SerializeObject(bonusCards, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        //        string jason_encrypt = CryptorEngine.Encrypt(jason, true, key);
        //        result = CompressString(jason_encrypt);                
        //    }
        //    catch (Exception ex)
        //    {
        //        //result = ex.Message;
        //    }
        //    finally
        //    {
        //        if (conn.State == ConnectionState.Open)
        //        {
        //            conn.Close();
        //        }
        //    }

        //    return result;
        //}



        #endregion


        [WebMethod]
        public DateTime GetDateTimeServer()
        {
            return DateTime.Now;
        }
        
        [WebMethod]
        public string SetStatusSertificat(string nick_shop, string data, string scheme)
        {
            string result = "1";

            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                return result;
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();
            string decrypt_data = CryptorEngine.Decrypt(data.ToString(), true, key);

            string[] list_sertificate = decrypt_data.ToString().Split('|');

            SqlConnection conn = new SqlConnection(scheme == "1" ? getConnectionString() : getConnectionString2());
            SqlTransaction trans = null;
            SqlCommand command = null;
            string query = "";
            string is_active = "";
            string activation="";
            string deactivation="";
            string descripton = nick_shop + " " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");


            try
            {
                conn.Open();
                trans = conn.BeginTransaction();

                foreach (string str in list_sertificate)
                {

                    string[] param = str.Split(',');                    

                    if (decimal.Parse(param[2].Replace(".",",")) > 0)
                    {
                        if (param[6] == "0")//Это продажа сертификата
                        {
                            is_active = "1";
                            activation = "'" + param[0] + " " + descripton + "'";
                            deactivation = "''";
                        }
                        else//Это возврат сертификата
                        {
                            is_active = "0";
                            activation = "''";
                            deactivation = "'" + param[0] + " " + descripton + "'";
                        }
                    }
                    else
                    {
                        is_active = "0";
                        activation = "''";
                        deactivation = "'" + param[0] + " " + descripton + "'";
                    }
                    
                    query = " UPDATE certificate SET is_active =" + 
                        is_active + ",activation=" + 
                        activation + ",deactivation="+
                        //deactivation+"  WHERE code_tovar = " + param[1];                    
                        deactivation + "  WHERE code = " + param[4];//Теперь сюда приезжает штрихкод
                    command = new SqlCommand(query, conn);
                    command.Transaction = trans;
                    command.ExecuteNonQuery();

                    query = "INSERT INTO certificate_log(" +
                    " code" + "," +
                    " code_tovar" + "," +
                    " action" + "," +
                    " shop" + "," +
                    " num_cash" + "," +
                    " num_doc" + "," +
                    " date_time_write" + "," +
                    " date_time_unloading) VALUES(" +
                    param[4] + "," +
                    param[1] + "," +
                    (deactivation == "''" ? "1" : "-1") + ",'" +
                    nick_shop + "'," +
                    param[3] + "," +
                    param[0] + ",'" +
                    param[5] + "','" +
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + "')";

                    command = new SqlCommand(query, conn);
                    command.Transaction = trans;
                    command.ExecuteNonQuery();                    
                }
                trans.Commit();
                conn.Close();

            }
            catch (SqlException ex)
            {
                if (trans != null)
                {
                    trans.Rollback();
                }
                result = "-1";                
            }
            catch (Exception)
            {
                if (trans != null)
                {
                    trans.Rollback();
                }
                result = "-1";                
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return result;
        }


        [WebMethod]
        public string GetDiscountClientsV8DateTime_NEW(string nick_shop, string data,string scheme)
        {
            SqlConnection conn = null; string result = "-1";

            //if (!check_avalible_dataV8())
            //{
            //    return result;
            //}

            StringBuilder sb = new StringBuilder();

            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                return result;
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();
            string[] delimiters = new string[] { "|" };
            string decrypt_data = CryptorEngine.Decrypt(data.ToString(), true, key);
            string[] d = decrypt_data.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            if (d.Length != 3)
            {
                return result;
            }

            if ((d[0].Trim() != nick_shop) || (d[2].Trim() != code_shop))
            {
                return result;
            }

            DateTime datetime = new DateTime(Convert.ToInt64(d[1]));

            //conn = new SqlConnection(getConnectionString2());
            conn = new SqlConnection(scheme == "1" ? getConnectionString() : getConnectionString2());
            try
            {
                StringBuilder result_query = new StringBuilder();
                conn.Open();
                //string query = "SELECT code,name,sum,birthday,type_card,its_work  FROM clients WHERE datetime_update > '" + datetime.AddDays(-1).ToString("dd-MM-yyyy HH:mm:ss") + "'";
                string query = " SELECT TOP 10000 code,name,sum,birthday,type_card,its_work,datetime_update,phone,attribute,bonus_is_on,reason_for_blocking,notify_security  FROM clients WHERE datetime_update >= '" + datetime.ToString("dd-MM-yyyy HH:mm:ss") + "' order by datetime_update ";
                SqlCommand command = new SqlCommand(query, conn);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result_query.Append("|'" + reader[0].ToString() + "','" + reader[1].ToString().Replace(",", "") + "'," +
                         reader[2].ToString().Replace(",", ".") + ",'" + reader.GetDateTime(3).ToString("yyyy-MM-dd") + "'," +
                         reader[4].ToString() + "," + reader[5].ToString() + ",'" + reader.GetDateTime(6).ToString("yyyy-MM-dd HH:mm:ss") + "','" +
                         (reader[7].ToString().Trim() == "" ? "0" : reader[7].ToString()) + "','" +
                         reader[8].ToString().Trim() + "','" + reader["bonus_is_on"].ToString().Trim() + "','" + 
                         reader["reason_for_blocking"].ToString().Trim()+ "','" +
                         (reader["notify_security"].ToString().Trim()=="False" ? "0" : "1")+"'");
                }
                reader.Close();
                conn.Close();

                result = CryptorEngine.Encrypt(result_query.ToString(), true, key);
            }
            catch (Exception ex)
            {
                //result = ex.Message;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return result;
        }



        [WebMethod]
        public string ExistsUpdateProrgam(string nick_shop, string data, string scheme)
        {
            string result = "";

            //nick_shop = "11109";
            //data = "VCPMWuAQ8D64pJYjn+hEhUIaP45IBFswIr9XTnVFFXwm+Vv+F/xYuvPr/d0PzUNuZ6xqngzZmjQ/ruEhqu203kRBYwcx+n2WJwriXYPC6sKBDtlwgQ6Je6/HloILcCUp";

            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                return result;
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();
            string decrypt_data = CryptorEngine.Decrypt(data.ToString(), true, key);
            string[] d = decrypt_data.ToString().Split('|');

            string path_for_distr = "C:\\DistrCashProgram\\Russia\\Cash8.exe";
            if (File.Exists(path_for_distr))
            {
                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo("C:\\DistrCashProgram\\Russia\\Cash8.exe");
                string cash_version = myFileVersionInfo.FileVersion;
                //Новое решение 
                Int64 local_version = Convert.ToInt64(cash_version.Replace(".", ""));
                Int64 remote_version = Convert.ToInt64(d[1].Replace(".", ""));
                if (local_version > remote_version)
                {
                    result = cash_version;
                }
                else
                {
                    result = d[1];
                }

                result = CryptorEngine.Encrypt(result, true, key);

            }

            return result;
        }

            //[WebMethod]
            //public string ExistsUpdateProrgam(string nick_shop, string data)
            //{
            //    string result = "";

            //    //nick_shop = "11109";
            //    //data = "VCPMWuAQ8D64pJYjn+hEhUIaP45IBFswIr9XTnVFFXwm+Vv+F/xYuvPr/d0PzUNuZ6xqngzZmjQ/ruEhqu203kRBYwcx+n2WJwriXYPC6sKBDtlwgQ6Je6/HloILcCUp";

            //    string code_shop = get_id_database(nick_shop, "1");
            //    if (code_shop.Trim().Length == 0)
            //    {
            //        return result;
            //    }

            //    string count_day = CryptorEngine.get_count_day();
            //    string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();
            //    string decrypt_data = CryptorEngine.Decrypt(data.ToString(), true, key);
            //    string[] d = decrypt_data.ToString().Split('|');

            //    string path_for_distr = "C:\\DistrCashProgram\\Russia\\Cash8.exe";
            //    if (File.Exists(path_for_distr))
            //    {
            //        FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo("C:\\DistrCashProgram\\Russia\\Cash8.exe");
            //        string cash_version = myFileVersionInfo.FileVersion;
            //        //Новое решение 
            //        Int64 local_version = Convert.ToInt64(cash_version.Replace(".", ""));
            //        Int64 remote_version = Convert.ToInt64(d[1].Replace(".", ""));
            //        if (local_version > remote_version)
            //        {
            //            result = cash_version;
            //        }
            //        else
            //        {
            //            result = d[1];
            //        }

            //        result = CryptorEngine.Encrypt(result, true, key);
            //    }

            //    return result;
            //}

        [WebMethod]
        public byte[] GetUpdateProgram(string nick_shop, string data, string scheme)
        {
            byte[] result = new byte[0];

            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                return result;
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();
            string decrypt_data = CryptorEngine.Decrypt(data.ToString(), true, key);
            string[] d = decrypt_data.ToString().Split('|');
            string path_for_distr = "C:\\DistrCashProgram\\Russia\\Cash8.exe";
            if (File.Exists(path_for_distr))
            {
                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo("C:\\DistrCashProgram\\Russia\\Cash8.exe");
                string cash_version = myFileVersionInfo.FileVersion;
                Int64 local_version = Convert.ToInt64(cash_version.Replace(".", ""));
                Int64 remote_version = Convert.ToInt64(d[1].Replace(".", ""));
                if (local_version == remote_version)
                {
                    result = File.ReadAllBytes(path_for_distr);
                }
            }

            return result;
        }

        //[WebMethod]
        //public byte[] GetUpdateProgram(string nick_shop, string data)
        //{
        //    byte[] result = new byte[0];

        //    string code_shop = get_id_database(nick_shop, "1");
        //    if (code_shop.Trim().Length == 0)
        //    {
        //        return result;
        //    }

        //    string count_day = CryptorEngine.get_count_day();
        //    string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();
        //    string decrypt_data = CryptorEngine.Decrypt(data.ToString(), true, key);
        //    string[] d = decrypt_data.ToString().Split('|');
        //    string path_for_distr = "C:\\DistrCashProgram\\Russia\\Cash8.exe";
        //    if (File.Exists(path_for_distr))
        //    {
        //        FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo("C:\\DistrCashProgram\\Russia\\Cash8.exe");
        //        string cash_version = myFileVersionInfo.FileVersion;
        //        Int64 local_version = Convert.ToInt64(cash_version.Replace(".", ""));
        //        Int64 remote_version = Convert.ToInt64(d[1].Replace(".", ""));
        //        if (local_version == remote_version)
        //        {
        //            result = File.ReadAllBytes(path_for_distr);
        //        }
        //    }

        //    return result;
        //}



        [WebMethod]
        public byte[] GetNpgsqlNew(string nick_shop, string data, string scheme)
        {
            byte[] result = new byte[0];

            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                return result;
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();
            string decrypt_data = CryptorEngine.Decrypt(data.ToString(), true, key);
            string[] d = decrypt_data.ToString().Split('|');
            string path_for_distr = "C:\\DistrCashProgram\\Npgsql\\Npgsql.dll";
            if (File.Exists(path_for_distr))
            {
                //FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo("C:\\DistrCashProgram\\Npgsql\\Npgsql.dll");
                //string cash_version = myFileVersionInfo.FileVersion;
                //Int64 local_version = Convert.ToInt64(cash_version.Replace(".", ""));
                //Int64 remote_version = Convert.ToInt64(d[1].Replace(".", ""));
                if (d[0] == d[1])
                {
                    result = File.ReadAllBytes(path_for_distr);
                }
            }

            return result;
        }

        [WebMethod]
        public string GetStatusSertificat(string nick_shop, string data, string scheme)
        {
            string result = "-1";                
            
            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                return result;
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();                
            string decrypt_data = CryptorEngine.Decrypt(data.ToString(), true, key);
            SqlConnection conn = new SqlConnection(scheme == "1" ? getConnectionString() : getConnectionString2());
            try
            {
                string query = "SELECT is_active FROM certificate WHERE code="+decrypt_data;
                conn.Open();
                SqlCommand command = new SqlCommand(query,conn);
                object result_query = command.ExecuteScalar();
                if (result_query != null)
                {
                    result = CryptorEngine.Encrypt(result_query.ToString(),true,key);
                }
            }
            catch
            {

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return result;
        }
      
        private string getConnectionString()
        {
            string result = "Server=192.168.2.59;User Id=cash-place;Password=ljcneg5116602014xbcnsqljv;Database=cash_8;"; //РАБОЧАЯ
            //string result = "Server=127.0.0.1;User Id=cash-place;Password=ljcneg5116602014xbcnsqljv;Database=cash_8;";
            //string result = "Server=10.21.200.78;User Id=cash-place;Password=ljcneg5116602014xbcnsqljv;Database=cash_8;"; //тестовый адрес EVA
            //string result = "Server=10.21.200.21\\V81C;User Id=cash-place;Password=ljcneg5116602014xbcnsqljv;Database=cash_8;"; //тестовый адрес EVA

            return result;
        }
        private string getConnectionString2()
        {
            string result = "Server=192.168.2.59;User Id=cash-place;Password=ljcneg5116602014xbcnsqljv;Database=cash_eva;"; //РАБОЧАЯ
            //string result = "Server=AP-35\\SQLEXPRESS;User Id=ch_disc_sql_tranzit;Password=511660;Database=ch_disc_tranzit;";
            //string result = "Server=127.0.0.1;User Id=cash-place;Password=ljcneg5116602014xbcnsqljv;Database=cash_eva;";
            //string result = "Server=10.21.6.2;User Id=ch_disc_sql_tranzit;Password=Sql0412755;Database=cash;";
            //string result = "Server=10.21.6.2;User Id=cash-place;Password=ljcneg5116602014xbcnsqljv;Database=cash_8;";

            return result;
        }

        private string getConnectionString4()
        {
            //string result = "Server=AP-35\\SQLEXPRESS;User Id=ch_disc_sql_tranzit;Password=511660;Database=ch_disc_tranzit;";
            string result = "Server=192.168.2.59;User Id=tsd;Password=clearhouse2016511660;Database=cash_8;";
            //string result = "Server=127.0.0.1;User Id=ch_disc_sql_tranzit;Password=Sql0412755;Database=cash;";

            return result;
        }

        private string getConnectionString5()
        {
            //string result = "Server=AP-35\\SQLEXPRESS;User Id=ch_disc_sql_tranzit;Password=511660;Database=ch_disc_tranzit;";
            string result = "Server=192.168.2.59;User Id=cash-place;Password=ljcneg5116602014xbcnsqljv;Database=time_logging;";
            //string result = "Server=127.0.0.1;User Id=ch_disc_sql_tranzit;Password=Sql0412755;Database=cash;";

            return result;
        }
        
        /// <summary>
        /// 1  онлайн
        /// 0  офлайн
        /// -1 ошибка такой возврат может быть и открытым если не удалось получить код магазина
        /// -2 нет сотрудника с таким кодом 
        /// -3 не найден магазин с таким кодом
        /// </summary>
        /// <param name="nick_shop"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [WebMethod]
        public string ChangeStatusWorkerOnline(string nick_shop, string data, string scheme)
        {
            string result = "";
            string worker_name = "";


            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                result = "-3";
                return result;
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();

            string[] delimiters = new string[] { "|" };
            string decrypt_data = CryptorEngine.Decrypt(data.ToString(), true, key);
            string[] d = decrypt_data.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

           // string worker_code = d[1];
            string worker_code = get_workers_code(d[1]);
            if (worker_code == "")
            {
                result = "-1";
            }

            string code_event = "";
            string code_event_insert = "";

            //string shop="";
            //string online = "";//1 онлайн 0 офлайн
            //string date_time_event = "";

            SqlConnection conn = new SqlConnection(getConnectionString5());
            SqlTransaction tran = null;
            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                string query = "SELECT COUNT(*) FROM events WHERE worker_code = '" + worker_code + "'";
                SqlCommand command = new SqlCommand(query, conn);
                command.Transaction = tran;
                long count_events = Convert.ToInt64(command.ExecuteScalar());

                if (count_events > 0)
                {
                    query = "SELECT SUM(event) FROM events WHERE worker_code = '" + worker_code + "'";
                    command = new SqlCommand(query, conn);
                    command.Transaction = tran;
                    count_events = Convert.ToInt64(command.ExecuteScalar());
                }

                //if (count_events != null)
                //{
                //code_event = result_query.ToString();
                if (count_events == 0)
                {
                    query = "UPDATE  workers SET status=1 WHERE code = '" + worker_code + "'";
                    code_event = "1";
                }
                else
                {
                    query = "UPDATE  workers SET status=0 WHERE code = '" + worker_code + "'";
                    code_event = "-1";
                }

                command = new SqlCommand(query, conn);
                command.Transaction = tran;
                command.ExecuteNonQuery();

                query = "INSERT INTO events(event,worker_code,date_time,shop)VALUES(" +
                code_event + ",'" +
                worker_code + "','" +
                DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + "','" +
                nick_shop + "')";

                command = new SqlCommand(query, conn);
                command.Transaction = tran;
                command.ExecuteNonQuery();

                query = "SELECT worker FROM workers WHERE code = '" + worker_code + "'";
                command = new SqlCommand(query, conn);
                command.Transaction = tran;
                worker_name = command.ExecuteScalar().ToString();
                //}
                //else
                //{
                //    code_event = "-2";
                //}

                tran.Commit();
                conn.Close();
            }
            catch (SqlException)
            {
                if (tran != null)
                {
                    tran.Rollback();
                }
                result = "-1";
                code_event = "-1";
            }
            catch (Exception)
            {
                if (tran != null)
                {
                    tran.Rollback();
                }
                result = "-1";
                code_event = "-1";
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            code_event += "|" + worker_name;

            result = CryptorEngine.Encrypt(code_event, true, key);

            return result;
        }
        
        /// <summary>
        /// 0 нет такой записи
        /// 1 есть такая запись
        /// -1 ошибка обработки
        /// </summary>
        /// <returns></returns>
        private int check_data_offline_record(string record, string nick_shop)
        {
            int result = 0;

            string[] delimiters = new string[] { "," };
            string[] d = record.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            SqlConnection conn = new SqlConnection(getConnectionString5());

            try
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM events WHERE worker_code='" 
                    + d[0] + "' AND date_time='" + d[1] + "' AND shop='" + nick_shop+"'";
                SqlCommand command = new SqlCommand(query, conn);
                result = Convert.ToInt16(command.ExecuteScalar());
                conn.Close();
            }
            catch
            {
                result = -1;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            
            return result;
        }
        
        private string get_workers_code(string barcode)
        {
            string result = "";

            SqlConnection conn = new SqlConnection(getConnectionString5());

            try
            {
                conn.Open();
                string query = "SELECT code FROM workers WHERE barcode='"+barcode+"'";
                SqlCommand command = new SqlCommand(query, conn);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result = reader["code"].ToString();                    
                }
                reader.Close();
                conn.Close();
            }
            catch
            {

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            

            return result; 
        }
        
        /// <summary>
        /// вставка записи 
        /// 1  Успех
        /// -1 Ошибка
        /// </summary>
        /// <param name="record"></param>
        /// <param name="nick_shop"></param>
        /// <returns></returns>
        private int insert_data_offline_record(string record,string nick_shop)
        {
            int result = 1;

            string[] delimiters = new string[] { "," };
            string[] d = record.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            SqlConnection conn = new SqlConnection(getConnectionString5());
            SqlTransaction tran = null;
            try
            {
                conn.Open();
                tran = conn.BeginTransaction();

                //worker_code заменяем
                string code = get_workers_code(d[0]);
                if (code == "")
                {
                    //result = -1;
                    //return result;
                    code = "99999999999";
                }

                //string query = "INSERT INTO events(event,worker_code,date_time,shop)VALUES(" +                    
                //    d[2]+ ",'" +
                //    d[0]+ "','" +
                //    d[1]+"','" + 
                //    nick_shop+"')";       
                //СНАЧАЛА УДАЛИМ 
                string query = "DELETE events WHERE date_time ='" + d[1] + "'  AND shop='" + nick_shop + "' and worker_code=" + d[0];
                SqlCommand command = new SqlCommand(query, conn);
                command.Transaction = tran;
                command.ExecuteNonQuery();
                //КОНЕЦ сНАЧАЛА УДАЛИМ

                query = "INSERT INTO events(event,worker_code,date_time,shop)VALUES(" +
                   d[2] + ",'" +
                   code + "','" +
                   d[1] + "','" +
                   nick_shop + "')";       
                command = new SqlCommand(query, conn);
                command.Transaction = tran;
                command.ExecuteNonQuery();

//                query = "SELECT SUM(event) FROM events WHERE worker_code = '" + d[0] + "'";
                query = "SELECT SUM(event) FROM events WHERE worker_code = '" + code + "'";
                command = new SqlCommand(query, conn);
                command.Transaction = tran;
                object result_query = command.ExecuteScalar();
                if (result_query != null)
                {
                    if (Convert.ToInt16(result_query) == 0)
                    {
                        query = "UPDATE  workers SET status=1 WHERE code = '" + code + "'";
                    }
                    else
                    {
                        query = "UPDATE  workers SET status=0 WHERE code = '" + code + "'";
                    }

                    command = new SqlCommand(query, conn);
                    command.Transaction = tran;
                    command.ExecuteNonQuery();
                }
                tran.Commit();
                conn.Close();
            }
            catch (SqlException)
            {
                if (tran != null)
                {
                    tran.Rollback();
                }
                result = -1;
            }
            catch (Exception)
            {
                if (tran != null)
                {
                    tran.Rollback();
                }
                result = -1;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return result;
        }
        
        [WebMethod]
        public int ChangeStatusWorkerOffline(string nick_shop, string data, string scheme)
        {
            int result = 1;
            //string worker_name = "";


            string code_shop = get_id_database(nick_shop, scheme);

            if (code_shop.Trim().Length == 0)
            {
                result = -3;
                return result;
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();

            string[] delimiters = new string[] { "||" };
            string decrypt_data = CryptorEngine.Decrypt(data.ToString(), true, key);
            string[] d = decrypt_data.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            delimiters = new string[] { "|" };
            string[] d2 = d[1].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            SqlConnection conn = new SqlConnection(getConnectionString5());

            try
            {
                foreach (string str in d2)
                {
                    //1. Проверка наличия уже такой записи
                    int result_check = check_data_offline_record(str, nick_shop);
                    if (result_check == 0)//нет такой записи надо вставить
                    {
                        int result_insert = insert_data_offline_record(str, nick_shop);
                        if (result_insert == -1)
                        {
                            result = -1;
                            return result;
                        }
                    }
                    else if (result_check == -1) //Произошли ошибки выходим из обработки 
                    {
                        result = -1;
                        return result;
                    }
                }
            }

            catch (Exception)
            {
                result = -1;
            }
            finally
            {

            }

            return result;
        }
                       
        private bool execute_insert_query(string query,int variant,string scheme)
        {
            bool result = false;

            SqlConnection conn = new SqlConnection(scheme == "1" ? getConnectionString() : getConnectionString2());

            SqlTransaction tran = null;

            try
            {             

                conn.Open();
                tran = conn.BeginTransaction();
                SqlCommand command = new SqlCommand(query, conn);
                command.Transaction = tran;
                command.ExecuteNonQuery();
                tran.Commit();
                conn.Close();
                command.Dispose();
                result = true;
            }
            catch(Exception ex)
            {
                if (tran != null)
                {
                    tran.Rollback();
                }
                result = false;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            
            return result;
        }

        private bool check_avalible_dataV8(string scheme)
        {
            SqlConnection conn = new SqlConnection(scheme == "1" ? getConnectionString() : getConnectionString2());
            bool result = false;

            try
            {
                conn.Open();
                string query = "SELECT exchange FROM constants";
                SqlCommand command = new SqlCommand(query, conn);
                object result_query = command.ExecuteScalar();
                if (result_query != null)
                {
                    result = Convert.ToBoolean(result_query);                    
                }
                //File.AppendAllText("C:\\DistrCashProgram\\Russia\\Test.txt", (result_query == null).ToString()+"\r\n");
            }
            catch(Exception ex)
            {                
                
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            //File.AppendAllText("C:\\DistrCashProgram\\Russia\\Test.txt", "Возвращаемое значение "+result.ToString()+"\r\n");
            return result;
        }

        public class ChangeStatusClients : IDisposable
        {
            public string NickShop { get; set; }
            public string NumCash { get; set; }
            public List<ChangeStatusClient> ListChangeStatusClient { get; set; }

            void IDisposable.Dispose()
            {

            }
        }

        public class ChangeStatusClient
        {          
            public string Client { get; set; }
            public string DateTimeChangeStatus { get; set; }
            public string new_phone_number { get; set; }

        }
        
        [WebMethod]
        public string UploadChangeStatusClients(string nick_shop, string data, string scheme)
        {
            string result = "-1";

            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                return result;
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();
            string decrypt_data = CryptorEngine.Decrypt(data, true, key);
            ChangeStatusClients changeStatusClients = JsonConvert.DeserializeObject<ChangeStatusClients>(decrypt_data);
            StringBuilder sb = new StringBuilder();
            foreach (ChangeStatusClient changeStatusClient in changeStatusClients.ListChangeStatusClient)
            {
                sb.Append("DELETE FROM client_changed_type WHERE "+
                    "client='"+ changeStatusClient.Client+"' AND "+
                    "shop='"+ changeStatusClients.NickShop+"' AND "+
                    "num_cash="+changeStatusClients.NumCash+" AND "+
                    "date_time='"+changeStatusClient.DateTimeChangeStatus+"' AND "+
                    "new_phone_number='"+changeStatusClient.new_phone_number+"';");
                sb.Append("INSERT INTO client_changed_type(" +
                    "date_time" +
                    ",client" +
                    ",shop" +
                    ",num_cash" +
                    ",processed,"+
                    "new_phone_number) VALUES('" +
                    changeStatusClient.DateTimeChangeStatus + "','"+
                    changeStatusClient.Client + "','" +
                    changeStatusClients.NickShop + "'," +
                    changeStatusClients.NumCash + "," +
                    "0,'"+
                    changeStatusClient.new_phone_number+"')");// processed)
            }

            SqlConnection conn = new SqlConnection(scheme == "1" ? getConnectionString() : getConnectionString2());
            SqlTransaction tran = null;
            try
            {
                if (sb.ToString().Trim() != "")
                {
                    conn.Open();
                    tran = conn.BeginTransaction();
                    string query = sb.ToString();
                    SqlCommand command = new SqlCommand(query, conn);
                    command.Transaction = tran;
                    command.ExecuteNonQuery();
                    tran.Commit();
                    command.Dispose();
                    conn.Close();
                }
                result = "1";
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return result;
        }

        public class PhoneClient
        {
            public string NumPhone { get; set; }
            public string ClientCode { get; set; }
        }


        public class PhonesClients : IDisposable
        {
            public string Version { get; set; }
            public string NickShop { get; set; }
            public string CodeShop { get; set; }
            public List<PhoneClient> ListPhoneClient { get; set; }

            void IDisposable.Dispose()
            {

            }
        }


        [WebMethod]
        public string UploadPhoneClients(string nick_shop, string data, string scheme)
        {
            string result = "-1";

            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                return result;
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();
            string decrypt_data = CryptorEngine.Decrypt(data, true, key);
            PhonesClients phonesClients = JsonConvert.DeserializeObject<PhonesClients>(decrypt_data);

            StringBuilder sb = new StringBuilder();
            foreach (PhoneClient phonesClient in phonesClients.ListPhoneClient)
            {
                sb.Append("DELETE FROM phone_number_in WHERE client_code='" + phonesClient.ClientCode + "';");
                sb.Append("INSERT INTO phone_number_in(client_code,phone_number)VALUES('" + phonesClient.ClientCode + "','" + phonesClient.NumPhone + "');");
                //sb.Append("DELETE FROM phone_number_log WHERE client_code='" + phonesClient.ClientCode + "';");
                sb.Append("INSERT INTO phone_number_log(shop,client_code,phone_number,date_time)VALUES('" + phonesClients.NickShop + "','" + phonesClient.ClientCode + "','"+phonesClient.NumPhone+"','"+ DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")+"');");
            }

            SqlConnection conn = new SqlConnection(scheme == "1" ? getConnectionString() : getConnectionString2());
            SqlTransaction tran = null;
            try
            {
                if (sb.ToString().Trim() != "")
                {
                    conn.Open();
                    tran = conn.BeginTransaction();
                    string query = sb.ToString();
                    SqlCommand command = new SqlCommand(query, conn);
                    command.Transaction = tran;
                    command.ExecuteNonQuery();
                    tran.Commit();
                    command.Dispose();
                    conn.Close();
                }
                result = "1";
            }
            catch(Exception ex)
            {

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                } 
            }
                       
            return result;
        }



        /// <summary>
        /// Уменьшенное количестов в чеке
        /// или удаленная строка
        /// </summary>
        public class DeletedItem
        {
            public string num_doc { get; set; }
            public string num_cash { get; set; }
            public string date_time_start { get; set; }
            public string date_time_action { get; set; }
            public string tovar { get; set; }
            public string quantity { get; set; }
            public string type_of_operation { get; set; }
        }

        public class DeletedItems : IDisposable
        {
            public string Version { get; set; }
            public string NickShop { get; set; }
            public string CodeShop { get; set; }
            public List<DeletedItem> ListDeletedItem { get; set; }

            void IDisposable.Dispose()
            {

            }
        }



        [WebMethod]
        public string UploadDeletedItems(string nick_shop, string data, string scheme)
        {

            string result = "-1";

            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                return result;
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();
            string decrypt_data = CryptorEngine.Decrypt(data, true, key);
            DeletedItems deletedItems = JsonConvert.DeserializeObject<DeletedItems>(decrypt_data);

            StringBuilder sb = new StringBuilder();
            foreach (DeletedItem deletedItem in deletedItems.ListDeletedItem)
            {
                sb.Append("DELETE FROM deleted_items WHERE " +
                    "shop='" + nick_shop +
                    "' AND num_doc=" + deletedItem.num_doc +
                    "  AND num_cash=" + deletedItem.num_cash +
                    "  AND date_time_start='" + deletedItem.date_time_start +
                    "' AND date_time_action='" + deletedItem.date_time_action +
                    "' AND tovar=" + deletedItem.tovar +
                    "  AND quantity=" + deletedItem.quantity +
                    "  AND type_of_operation=" + deletedItem.type_of_operation+";");

                sb.Append("INSERT INTO deleted_items" +
                    "    (shop" +
                    "    ,num_doc" +
                    "    ,num_cash" +
                    "    ,date_time_start" +
                    "    ,date_time_action" +
                    "    ,tovar" +
                    "    ,quantity" +
                    "    ,type_of_operation)" +
                    "    VALUES ('" +
                         nick_shop + "'," +
                         deletedItem.num_doc + "," +
                         deletedItem.num_cash + ",'" +
                          deletedItem.date_time_start + "','" +
                          deletedItem.date_time_action + "'," +
                          deletedItem.tovar + "," +
                          deletedItem.quantity + "," +
                          deletedItem.type_of_operation + ");");
            }

            SqlConnection conn = new SqlConnection(scheme == "1" ? getConnectionString() : getConnectionString2());
            SqlTransaction tran = null;
            try
            {
                if (sb.ToString().Trim() != "")
                {
                    conn.Open();
                    tran = conn.BeginTransaction();
                    string query = sb.ToString();
                    SqlCommand command = new SqlCommand(query, conn);
                    command.Transaction = tran;
                    command.ExecuteNonQuery();
                    tran.Commit();
                    command.Dispose();
                    conn.Close();
                }
                result = "1";
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return result;


        }
            

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nick_shop"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [WebMethod]
        public string UploadCodeClients(string nick_shop, string data, string scheme)
        {
            string result = "-1";

            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                return result;
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();
            string decrypt_data = CryptorEngine.Decrypt(data, true, key);
            string[] delimiters = new string[] { "|" };
            string[] delimiters2 = new string[] { "," };
            string[] d = decrypt_data.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder sb = new StringBuilder();
            foreach (string str in d)
            {
                string[] d2 = str.Split(delimiters2, StringSplitOptions.RemoveEmptyEntries);
                sb.Append("DELETE FROM new_client_code_in WHERE old_client_code=" + d2[0] + ";");
                sb.Append("INSERT INTO new_client_code_in(old_client_code,new_client_code,shop,date_record) VALUES(" + str + ");");
            }

            SqlConnection conn = new SqlConnection(getConnectionString());
            SqlTransaction tran = null;
            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                string query = sb.ToString();
                SqlCommand command = new SqlCommand(query, conn);
                command.Transaction = tran;
                command.ExecuteNonQuery();
                tran.Commit();
                command.Dispose();
                conn.Close();
                result = "1";
            }
            catch
            {

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return result;
        }

        #region GetDataForCasheV8Jason

        /// <summary>        
        /// Класс данных для отправки на кассу        
        /// </summary>
        public class LoadPacketData:IDisposable
        {
            public int Threshold { get; set; }
            public List<Tovar> ListTovar { get; set; }
            public List<Barcode> ListBarcode { get; set; }
            public List<ActionHeader> ListActionHeader { get; set; }
            public List<ActionTable> ListActionTable { get; set; }
            public List<Characteristic> ListCharacteristic { get; set; }
            public List<Sertificate> ListSertificate { get; set; }
            public List<PromoText> ListPromoText { get; set; }
            public List<ActionClients> ListActionClients { get; set; }
            public bool PacketIsFull { get; set; }//true если пакет заполннен до конца
            public bool Exchange { get; set; }//true если идет обмен
            public string Exception { get; set; }//true если идет обмен

            void IDisposable.Dispose()
            {
                
            }
        }

        public class Tovar:IDisposable
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string RetailPrice { get; set; }            
            public string ItsDeleted { get; set; }
            public string Nds { get; set; }
            public string ItsCertificate { get; set; }
            public string PercentBonus { get; set; }
            public string TnVed { get; set; }
            public string ItsMarked { get; set; }
            public string ItsExcise { get; set; }



            void IDisposable.Dispose()
            {
                
            }
        }
        public class Barcode:IDisposable
        {
            public string BarCode { get; set; }
            public string TovarCode { get; set; }

            void IDisposable.Dispose()
            {
             
            }
        }
        public class ActionHeader:IDisposable
        {
            public string DateStarted { get; set; }
            public string DateEnd { get; set; }
            public string NumDoc { get; set; }
            public string Tip { get; set; }
            public string Barcode { get; set; }
            public string Persent { get; set; }
            public string sum { get; set; }
            public string Comment { get; set; }
            //public string CodeTovar { get; set; }
            public string Marker { get; set; }
            public string ActionByDiscount { get; set; }
            public string TimeStart { get; set; }
            public string TimeEnd { get; set; }
            public string BonusPromotion { get; set; }
            public string WithOldPromotion { get; set; }
            public string Monday { get; set; }
            public string Tuesday { get; set; }
            public string Wednesday { get; set; }
            public string Thursday { get; set; }
            public string Friday { get; set; }
            public string Saturday { get; set; }
            public string Sunday { get; set; }
            public string PromoCode { get; set; }
            public string SumBonus { get; set; }
            public string ExecutionOrder { get; set; }
            public string GiftPrice { get; set; }
            public string Kind { get; set; }


            void IDisposable.Dispose()
            {
                
            }
        }
        public class ActionTable:IDisposable
        {
            public string NumDoc { get; set; }
            public string NumList { get; set; }
            public string CodeTovar { get; set; }
            public string Price { get; set; }

            void IDisposable.Dispose()
            {
                
            }
        }
        public class Characteristic:IDisposable
        {
            public string CodeTovar { get; set; }
            public string Name { get; set; }
            public string Guid { get; set; }
            public string RetailPrice { get; set; }

            void IDisposable.Dispose()
            {
                
            }
        }
        public class Sertificate:IDisposable
        {
            public string Code { get; set; }
            public string CodeTovar { get; set; }
            public string Rating { get; set; }
            public string IsActive { get; set; }

            void IDisposable.Dispose()
            {
                
            }
        }
        public class PromoText:IDisposable
        {
            public string AdvertisementText { get; set; }
            public string NumStr { get; set; }

            void IDisposable.Dispose()
            {
                
            }
        }
        public class ActionClients:IDisposable
        {
            public string NumDoc { get; set; }
            public string CodeClient { get; set; }

            void IDisposable.Dispose()
            {
                
            }
        }
        public class QueryPacketData
        {
            public string Version { get; set; }
            public string NickShop { get; set; }
            public string CodeShop { get; set; }
            public string NumCash { get; set; }
            public string LastDateDownloadTovar { get; set; }
        }
        private Byte[] CompressString(string value)
        {
            Byte[] byteArray = new byte[0];
            if (!string.IsNullOrEmpty(value))
            {
                byteArray = Encoding.UTF8.GetBytes(value);
                using (MemoryStream stream = new MemoryStream())
                {
                    using (GZipStream zip = new GZipStream(stream, CompressionMode.Compress))
                    {
                        zip.Write(byteArray, 0, byteArray.Length);
                    }
                    byteArray = stream.ToArray();
                }
            }
            return byteArray;
        }

        /// <summary>
        /// Эта функция собирает информацию для кассы по ее коду
        /// 
        /// </summary>
        /// <param name="nick_shop"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [WebMethod]
        public Byte[] GetDataForCasheV8Jason(string nick_shop, string data,string scheme)
        {
            DateTime dt_start = DateTime.Now;
            //string result = "-1";
            Byte[] result = Encoding.UTF8.GetBytes("-1");
                        
            if (check_avalible_dataV8(scheme))
            {                
                //insert_errors_GetDataForCasheV8Jason(nick_shop, "2", "check_avalible_dataV8");                
                return result = Encoding.UTF8.GetBytes("-1");
            }
            
            string code_shop = get_id_database(nick_shop,scheme);
            if (code_shop.Trim().Length == 0)
            {
                //insert_errors_GetDataForCasheV8Jason(nick_shop, "2", " нема кода ");                
                return result = Encoding.UTF8.GetBytes("-2");
            }
            

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();           
            string decrypt_data = CryptorEngine.Decrypt(data, true, key);
            QueryPacketData queryPacketData = JsonConvert.DeserializeObject<QueryPacketData>(decrypt_data);

            

            SqlConnection conn = new SqlConnection( scheme=="1" ? getConnectionString() : getConnectionString2());
            using (LoadPacketData loadPacketData = new LoadPacketData())
            {
                if (Convert.ToDouble(queryPacketData.Version) < 10873126877)
                {
                    loadPacketData.Exception=" У вас устаревшая версия программы ";

                }
                loadPacketData.PacketIsFull = false;//Пакет полностью заполнен
                loadPacketData.Exchange = false;//В базе идет обновление данных

                try
                {

                    conn.Open();

                    string query = "IF OBJECT_ID('tempdb..#t_t_nick_shop_num_cash') IS NOT NULL DROP TABLE #t_t_nick_shop_num_cash; " +
                        " IF OBJECT_ID('tempdb..#t_t2_nick_shop_num_cash') IS NOT NULL DROP TABLE #t_t2_nick_shop_num_cash; " +
                        " IF OBJECT_ID('tempdb..#t_t3_nick_shop_num_cash') IS NOT NULL DROP TABLE #t_t3_nick_shop_num_cash; " +
                        " SELECT prices_by_price_types.tovar_code AS tovar_code, prices_by_price_types.price AS prices_by_price_type INTO #t_t_nick_shop_num_cash from shops " +
                        " LEFT JOIN prices_by_price_types ON shops.UID_type_of_prices = prices_by_price_types.UID_type_of_prices " +
                        " WHERE shops.code = @nick_shop;" +
                        " SELECT tovar.code,prices.price AS personal_price INTO #t_t2_nick_shop_num_cash " +
                        " FROM tovar LEFT JOIN prices  ON tovar.code = prices.tovar_code AND shop = @nick_shop  AND prices.characteristic IS NULL  GROUP BY code,price " +
                        " HAVING ISNULL(price, 0) > 0;" +
                        " CREATE CLUSTERED INDEX tovar_code_index ON #t_t_nick_shop_num_cash (tovar_code ASC);" +
                        " CREATE CLUSTERED INDEX code_index ON #t_t2_nick_shop_num_cash (code ASC);" +
                        " SELECT tovar.name,tovar.its_deleted,tovar.nds,tovar.its_certificate,tovar.percent_bonus,tovar.tnved,tovar.its_marked,tovar.its_excise," +
                        " COALESCE(#t_t2_nick_shop_num_cash.code, #t_t_nick_shop_num_cash.tovar_code) AS code," +
                        " CASE WHEN #t_t2_nick_shop_num_cash.personal_price IS NOT NULL THEN " +
                        " #t_t2_nick_shop_num_cash.personal_price " +
                        " ELSE " +
                        " #t_t_nick_shop_num_cash.prices_by_price_type " +
                        " END AS price  INTO #t_t3_nick_shop_num_cash " +
                        " FROM  tovar LEFT JOIN #t_t_nick_shop_num_cash ON tovar.code =  #t_t_nick_shop_num_cash.tovar_code LEFT JOIN #t_t2_nick_shop_num_cash ON tovar.code =  #t_t2_nick_shop_num_cash.code " +
                        " WHERE #t_t_nick_shop_num_cash.prices_by_price_type IS NOT NULL  OR #t_t2_nick_shop_num_cash.personal_price IS NOT NULL;" +
                        " IF OBJECT_ID('tempdb..#t_t_nick_shop_num_cash') IS NOT NULL  DROP TABLE #t_t_nick_shop_num_cash;" +
                        " IF OBJECT_ID('tempdb..#t_t2_nick_shop_num_cash') IS NOT NULL  DROP TABLE #t_t2_nick_shop_num_cash;" +
                        " CREATE CLUSTERED INDEX code_index ON #t_t3_nick_shop_num_cash (code ASC);";

                    query = query.Replace("_nick_shop_num_cash", nick_shop + "_" + queryPacketData.NumCash);
                    query = query.Replace("@nick_shop", "'" + nick_shop + "'");

                    //INTO #t_t3_nick_shop_num_cash  
                    SqlCommand command = new SqlCommand(query, conn);
                    command.CommandTimeout = 120;
                    command.ExecuteNonQuery();


                    query = " SELECT * FROM #t_t3_nick_shop_num_cash ";
                    query = query.Replace("_nick_shop_num_cash", nick_shop + "_" + queryPacketData.NumCash);

                    command = new SqlCommand(query, conn);
                    command.CommandTimeout = 120;
                    SqlDataReader reader = command.ExecuteReader();

                    loadPacketData.ListTovar = new List<Tovar>();
                    while (reader.Read())
                    {

                        using (Tovar tovar = new Tovar())
                        {
                            tovar.Code = reader["code"].ToString();
                            tovar.Name = reader["name"].ToString();
                            tovar.RetailPrice = reader["price"].ToString().Replace(",", ".");
                            tovar.ItsDeleted = reader["its_deleted"].ToString();
                            tovar.Nds = reader["nds"].ToString();
                            tovar.ItsCertificate = reader["its_certificate"].ToString();
                            tovar.PercentBonus = reader["percent_bonus"].ToString().Replace(",", ".");
                            tovar.TnVed = reader["tnved"].ToString();
                            tovar.ItsMarked = reader["its_marked"].ToString();
                            tovar.ItsExcise = (Convert.ToBoolean(reader["its_excise"]) == false ? "0" : "1");
                            loadPacketData.ListTovar.Add(tovar);
                        }
                    }
                    reader.Close();

                    query = " SELECT barcode.tovar_code, barcode.barcode  FROM barcode WHERE barcode.tovar_code in(" +
                            " SELECT #t_t3_nick_shop_num_cash.code FROM #t_t3_nick_shop_num_cash) ;";
                    //" IF OBJECT_ID('tempdb..#t_t3_nick_shop_num_cash') IS NOT NULL DROP TABLE #t_t3_nick_shop_num_cash;";
                    query = query.Replace("_nick_shop_num_cash", nick_shop + "_" + queryPacketData.NumCash);

                    //query = " SELECT barcode.tovar_code, barcode.barcode  FROM barcode"; 

                    command = new SqlCommand(query, conn);
                    command.CommandTimeout = 120;
                    reader = command.ExecuteReader();

                    loadPacketData.ListBarcode = new List<Barcode>();
                    while (reader.Read())
                    {
                        using (Barcode barcode = new Barcode())
                        {
                            barcode.TovarCode = reader["tovar_code"].ToString();
                            barcode.BarCode = reader["barcode"].ToString();
                            loadPacketData.ListBarcode.Add(barcode);
                        }
                    }
                    reader.Close();

                    query = " SELECT date_start,date_end,action_active.num_doc,tip,barcode,persent,sum,comment, " +
                            " mark,disc_only,time_start,time_end " +//" present,mark,disc_only,time_start,time_end " +
                            " ,bonus_promotion,with_old_promotion,day_mon,day_tue" +
                            " ,day_wed,day_thu,day_fri,day_sat,day_sun,promo_code" +
                            " ,sum_bonus,execution_order,gift_price,kind " +
                            " FROM  (SELECT num_doc FROM action_active where shop='" + nick_shop + "') AS action_active " +
                            " LEFT JOIN action_header ON action_active.num_doc = action_header.num_doc ";

                    command = new SqlCommand(query, conn);
                    command.CommandTimeout = 120;
                    reader = command.ExecuteReader();

                    loadPacketData.ListActionHeader = new List<ActionHeader>();
                    while (reader.Read())
                    {
                        using (ActionHeader actionHeader = new ActionHeader())
                        {
                            actionHeader.DateStarted = reader.GetDateTime(0).ToString("yyyy-MM-dd");
                            actionHeader.DateEnd = reader.GetDateTime(1).ToString("yyyy-MM-dd");
                            actionHeader.NumDoc = reader["num_doc"].ToString();
                            actionHeader.Tip = reader["tip"].ToString();
                            actionHeader.Barcode = reader["barcode"].ToString();
                            //actionHeader.CodeTovar = reader["present"].ToString().Replace(",", ".");
                            //actionHeader.CodeTovar = "0";// reader["present"].ToString().Replace(",", ".");
                            actionHeader.Persent = reader["persent"].ToString().Replace(",", ".");
                            actionHeader.sum = reader["sum"].ToString().Replace(",", ".");
                            actionHeader.Comment = reader["comment"].ToString().Trim();
                            actionHeader.Marker = reader["mark"].ToString().Trim();
                            actionHeader.ActionByDiscount = reader["disc_only"].ToString().Trim();
                            actionHeader.TimeStart = reader["time_start"].ToString().Trim();
                            actionHeader.TimeEnd = reader["time_end"].ToString().Trim();
                            actionHeader.BonusPromotion = reader["bonus_promotion"].ToString().Trim();
                            actionHeader.WithOldPromotion = reader["with_old_promotion"].ToString().Trim();
                            actionHeader.Monday = reader["day_mon"].ToString().Trim();
                            actionHeader.Tuesday = reader["day_tue"].ToString().Trim();
                            actionHeader.Wednesday = reader["day_wed"].ToString().Trim();
                            actionHeader.Thursday = reader["day_thu"].ToString().Trim();
                            actionHeader.Friday = reader["day_fri"].ToString().Trim();
                            actionHeader.Saturday = reader["day_sat"].ToString().Trim();
                            actionHeader.Sunday = reader["day_sun"].ToString().Trim();
                            actionHeader.PromoCode = reader["promo_code"].ToString().Trim();
                            actionHeader.SumBonus = reader["sum_bonus"].ToString().Trim();
                            actionHeader.ExecutionOrder = reader["execution_order"].ToString().Trim();
                            actionHeader.GiftPrice = reader["gift_price"].ToString().Replace(",", ".");
                            actionHeader.Kind = reader["kind"].ToString();

                            loadPacketData.ListActionHeader.Add(actionHeader);
                        }
                    }
                    reader.Close();

                    query = " SELECT action_active.num_doc,action_table.num_list,action_table.tovar,action_table.price " +
                        " FROM  (SELECT num_doc FROM action_active where shop='" + nick_shop + "') AS action_active " +
                        " LEFT JOIN action_table ON action_active.num_doc = action_table.num_doc  WHERE action_table.tovar IN " +
                        "(SELECT #t_t3_nick_shop_num_cash.code FROM #t_t3_nick_shop_num_cash);" +
                        "IF OBJECT_ID('tempdb..#t_t3_nick_shop_num_cash') IS NOT NULL DROP TABLE #t_t3_nick_shop_num_cash;";

                    query = query.Replace("_nick_shop_num_cash", nick_shop + "_" + queryPacketData.NumCash);

                    command = new SqlCommand(query, conn);
                    //command.CommandTimeout = 300;
                    command.CommandTimeout = 120;
                    reader = command.ExecuteReader();

                    loadPacketData.ListActionTable = new List<ActionTable>();
                    while (reader.Read())
                    {
                        using (ActionTable actionTable = new ActionTable())
                        {
                            actionTable.NumDoc = reader["num_doc"].ToString();
                            actionTable.NumList = reader["num_list"].ToString();
                            actionTable.CodeTovar = reader["tovar"].ToString();
                            actionTable.Price = reader["price"].ToString().Replace(",", ".");
                            loadPacketData.ListActionTable.Add(actionTable);
                        }
                    }
                    reader.Close();

                    query = " SELECT characteristic.tovar_code, characteristic.guid, characteristic.name, prices.price AS retail_price" +
                        " FROM characteristic LEFT JOIN prices ON characteristic.guid = prices.characteristic  where shop = '" + nick_shop + "' AND prices.characteristic is not null " +
                        " GROUP BY characteristic.tovar_code, characteristic.guid, characteristic.name,prices.price";
                    command = new SqlCommand(query, conn);
                    command.CommandTimeout = 120;
                    reader = command.ExecuteReader();

                    loadPacketData.ListCharacteristic = new List<Characteristic>();
                    while (reader.Read())
                    {
                        using (Characteristic characteristic = new Characteristic())
                        {
                            characteristic.CodeTovar = reader["tovar_code"].ToString();
                            characteristic.Guid = reader["guid"].ToString();
                            characteristic.Name = reader["name"].ToString();
                            characteristic.RetailPrice = reader["retail_price"].ToString().Replace(",", ".");
                            loadPacketData.ListCharacteristic.Add(characteristic);
                        }
                    }
                    reader.Close();

                    query = "SELECT code,code_tovar,rating,is_active  FROM certificate";
                    command = new SqlCommand(query, conn);
                    command.CommandTimeout = 120;
                    reader = command.ExecuteReader();

                    loadPacketData.ListSertificate = new List<Sertificate>();
                    while (reader.Read())
                    {
                        using (Sertificate sertificate = new Sertificate())
                        {
                            sertificate.Code = reader["code"].ToString();
                            sertificate.CodeTovar = reader["code_tovar"].ToString();
                            sertificate.Rating = reader["rating"].ToString();
                            sertificate.IsActive = reader["is_active"].ToString();
                            loadPacketData.ListSertificate.Add(sertificate);
                        }
                    }
                    reader.Close();

                    query = " SELECT promo_active.code,shop,promo_text.text " +
                        " FROM promo_active  LEFT JOIN promo_text " +
                        " ON promo_active.code = promo_text.code " +
                        " where promo_active.shop = '" + nick_shop + "'" +
                        " order by promo_text.code ";
                    loadPacketData.ListPromoText = new List<PromoText>();
                    command = new SqlCommand(query, conn);
                    command.CommandTimeout = 120;
                    reader = command.ExecuteReader();

                    int numstr = 1;
                    while (reader.Read())
                    {
                        using (PromoText promoText = new PromoText())
                        {
                            promoText.AdvertisementText = reader["text"].ToString();
                            promoText.NumStr = numstr.ToString();
                            numstr++;
                            loadPacketData.ListPromoText.Add(promoText);
                        }
                    }
                    reader.Close();

                    query = " SELECT action_clients.num_doc,code_client FROM action_clients " +
                            " LEFT JOIN action_active ON action_clients.num_doc=action_active.num_doc AND action_active.shop='" + nick_shop + "'";
                    command = new SqlCommand(query, conn);
                    command.CommandTimeout = 120;
                    reader = command.ExecuteReader();

                    loadPacketData.ListActionClients = new List<ActionClients>();
                    while (reader.Read())
                    {
                        using (ActionClients actionClients = new ActionClients())
                        {
                            actionClients.NumDoc = reader["num_doc"].ToString();
                            actionClients.CodeClient = reader["code_client"].ToString();
                            loadPacketData.ListActionClients.Add(actionClients);
                        }
                    }
                    reader.Close();

                    query = "SELECT limit FROM constants";
                    command = new SqlCommand(query, conn);

                    loadPacketData.Threshold = Convert.ToInt32(command.ExecuteScalar());

                    conn.Close();

                    if (loadPacketData.Exception ==null)
                    {
                        loadPacketData.PacketIsFull = true;
                    }
                    if (check_avalible_dataV8(scheme))
                    {
                        loadPacketData.Exchange = true;
                    }
                    //System.IO.File.AppendAllText("C:\\DistrCashProgram\\Russia\\Test.txt", "1" + "\r\n");
                    //StringBuilder sb = new StringBuilder();
                    //StringWriter sw = new StringWriter(sb);
                    ////System.IO.File.AppendAllText("C:\\DistrCashProgram\\Russia\\Test.txt", "2" + "\r\n");
                    //using (JsonWriter textWriter = new JsonTextWriter(sw))
                    //{
                    //    var serializer = new JsonSerializer();
                    //    serializer.Serialize(textWriter, loadPacketData);
                    //}
                    //System.IO.File.AppendAllText("C:\\DistrCashProgram\\Russia\\Test.txt", "3" + "\r\n");
                    string jason = JsonConvert.SerializeObject(loadPacketData, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });                    

                    string jason_encrypt = CryptorEngine.Encrypt(jason, true, key);                    
                    result = CompressString(jason_encrypt);
                    
                    DateTime dt_finish = DateTime.Now;                    
                    query = "INSERT INTO stat (shop,date_time_begin ,date_time_end) VALUES " +
                        "('" + nick_shop + "','" + dt_start.ToString("dd-MM-yyyy HH:mm:ss") + "','" + dt_finish.ToString("dd-MM-yyyy HH:mm:ss") + "')";                    
                    execute_insert_query(query, 2,scheme);                    
                }
                catch (SqlException ex)
                {
                    //return "-1"; здесь ничего не делаем 
                    //loadPacketData.PacketIsFull останется false и так мы поймем , что пакет не полный
                    insert_errors_GetDataForCasheV8Jason(nick_shop, "1", ex.Message,scheme);
                    File.AppendAllText("C:\\DistrCashProgram\\Russia\\Test.txt", ex.Message + "\r\n");
                }
                catch (Exception ex)
                {
                    //return "-1"; здесь ничего не делаем 
                    //loadPacketData.PacketIsFull останется false и так мы поймем , что пакет не полный
                    insert_errors_GetDataForCasheV8Jason(nick_shop, "2", ex.Message,scheme);
                    File.AppendAllText("C:\\DistrCashProgram\\Russia\\Test.txt", ex.Message + "\r\n");
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }                
            }
            return result;
        }
        
        
        private void insert_errors_GetDataForCasheV8Jason(string shop,string num_cash,string info,string scheme)
        {
            SqlConnection conn = new SqlConnection(scheme == "1" ? getConnectionString() : getConnectionString2());
            try
            {
                conn.Open();
                string query = "INSERT INTO logs(date_time_write,shop,num_cash,info)VALUES('"+
                                                 DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")+"','"+
                                                 shop+"',"+
                                                 num_cash+",'"+
                                                 info+"')";
                SqlCommand command = new SqlCommand(query, conn);
                command.ExecuteNonQuery();
                conn.Close();
                command.Dispose();
            }
            catch(Exception ex)
            {
                
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        

        #endregion

        [WebMethod]
        public string GetDataForCasheV8Successfully(string nick_shop, string data,string scheme)
        {
            string result = "1";

            if (check_avalible_dataV8(scheme))
            {
                return result = "-1";
            }

            //Проверка доступа 
            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                return result = "-1";
            }

            if (nick_shop.Trim().Length == 0)
            {
                return result = "-1";
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + nick_shop.Trim();
            string decrypt_data = CryptorEngine.Decrypt(data, true, key);
            ResultGetData resultGetData = JsonConvert.DeserializeObject<ResultGetData>(decrypt_data);
            if (resultGetData.Successfully != "Successfully")
            {
                return result = "-1";
            }
            SqlConnection conn = new SqlConnection(scheme == "1" ? getConnectionString() : getConnectionString2());
            try
            {
                conn.Open();
                string query = "UPDATE cashbox SET version = " + resultGetData.Version + " ,date_time_import = '" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + "'," +
                                       "verOS='" + resultGetData.OSVersion +"',device_info='"+resultGetData.DeviceInfo+"' " +
                    " WHERE shop='" + nick_shop + "' AND num_cash=" + resultGetData.NumCash;
                SqlCommand command = new SqlCommand(query, conn);
                int result_update = command.ExecuteNonQuery();
                if (result_update == 0)
                {
                    query = "INSERT INTO cashbox (" +
                        "shop" +
                        ",num_cash" +
                        ",version" +
                        ",date_time_import"+
                        ",verOS"+
                        ",device_info)VALUES('" +
                        nick_shop + "'," +
                        resultGetData.NumCash + "," +
                        resultGetData.Version + ",'" +
                        DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + "','"+
                        resultGetData.OSVersion+"','" +
                        resultGetData.DeviceInfo+"')";
                    command = new SqlCommand(query, conn);
                    command.ExecuteNonQuery();
                }
                command.Dispose();
                conn.Close();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return result;
        }


        [WebMethod]
        public string OnlineCasheV8Successfully(string nick_shop, string data,string scheme)
        {
            string result = "1";

            if (check_avalible_dataV8(scheme))
            {
                return result = "-1";
            }

            //Проверка доступа 
            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                return result = "-1";
            }

            if (nick_shop.Trim().Length == 0)
            {
                return result = "-1";
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + nick_shop.Trim();
            string decrypt_data = CryptorEngine.Decrypt(data, true, key);
            ResultGetData resultGetData = JsonConvert.DeserializeObject<ResultGetData>(decrypt_data);
            if (resultGetData.Successfully != "Successfully")
            {
                return result = "-1";
            }
            
            SqlConnection conn = new SqlConnection(scheme == "1" ? getConnectionString() : getConnectionString2());
            try
            {
                conn.Open();
                string query = "UPDATE cashbox SET version = " + resultGetData.Version + " ,date_time_online = '" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + "'" +
                    " WHERE shop='" + nick_shop + "' AND num_cash=" + resultGetData.NumCash;
                SqlCommand command = new SqlCommand(query, conn);
                int result_update = command.ExecuteNonQuery();
                if (result_update == 0)
                {
                    query = "INSERT INTO cashbox (" +
                        "shop" +
                        ",num_cash" +
                        ",version" +
                        ",date_time_online)VALUES('" +
                        nick_shop + "'," +
                        resultGetData.NumCash + "," +
                        resultGetData.Version + ",'" +
                        DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + "')";
                    command = new SqlCommand(query, conn);
                    result_update = command.ExecuteNonQuery();
                }
                command.Dispose();
                conn.Close();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return result;
        }




        public class ResultGetData
        {
            public string Successfully { get; set; }
            public string Shop { get; set; }
            public string NumCash { get; set; }
            public string Version { get; set; }
            public string OSVersion { get; set; }
            public string DeviceInfo { get; set; }
        }

        /// <summary>
        /// Эта функция собирает информацию для кассы по ее коду
        /// 
        /// </summary>
        /// <param name="nick_shop"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [WebMethod]
        public Byte[] GetDataForCasheV8JasonUpdateOnly(string nick_shop, string data, string scheme)
        {
            //string result = "-1";
            Byte[] result = Encoding.UTF8.GetBytes("-1");


            if (check_avalible_dataV8(scheme))
            {
                return result = Encoding.UTF8.GetBytes("-1");
            }

            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                return result = Encoding.UTF8.GetBytes("-1");
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();
            string decrypt_data = CryptorEngine.Decrypt(data, true, key);
            QueryPacketData queryPacketData = JsonConvert.DeserializeObject<QueryPacketData>(decrypt_data);
            //queryPacketData.LastDateDownloadTovar

            SqlConnection conn = new SqlConnection(getConnectionString());
            using (LoadPacketData loadPacketData = new LoadPacketData())
            {
                loadPacketData.PacketIsFull = false;//Пакет полностью заполнен
                loadPacketData.Exchange = false;//В базе идет обновление данных

                try
                {

                    conn.Open();                  
                    string query = "IF OBJECT_ID('tempdb..#t_t_nick_shop_num_cash') IS NOT NULL DROP TABLE #t_t_nick_shop_num_cash; " +
                        " IF OBJECT_ID('tempdb..#t_t2_nick_shop_num_cash') IS NOT NULL DROP TABLE #t_t2_nick_shop_num_cash; " +
                        " IF OBJECT_ID('tempdb..#t_t3_nick_shop_num_cash') IS NOT NULL DROP TABLE #t_t3_nick_shop_num_cash; " +
                        " SELECT prices_by_price_types.tovar_code AS tovar_code, prices_by_price_types.price AS prices_by_price_type INTO #t_t_nick_shop_num_cash from shops " +
                        " LEFT JOIN prices_by_price_types ON shops.UID_type_of_prices = prices_by_price_types.UID_type_of_prices " +
                        " WHERE shops.code = @nick_shop AND prices_by_price_types.datetime_update>'" + queryPacketData.LastDateDownloadTovar + "';" +
                        " SELECT tovar.code,prices.price AS personal_price INTO #t_t2_nick_shop_num_cash " +
                        " FROM tovar LEFT JOIN prices  ON tovar.code = prices.tovar_code AND shop = @nick_shop  AND prices.characteristic IS NULL  "+
                        " WHERE prices.datetime_update>'" + queryPacketData.LastDateDownloadTovar + "' GROUP BY code,price " +
                        " HAVING ISNULL(price, 0) > 0;" +
                        " CREATE CLUSTERED INDEX tovar_code_index ON #t_t_nick_shop_num_cash (tovar_code ASC);" +
                        " CREATE CLUSTERED INDEX code_index ON #t_t2_nick_shop_num_cash (code ASC);" +
                        " SELECT tovar.name,tovar.its_deleted,tovar.nds,tovar.its_certificate,tovar.percent_bonus,tovar.tnved,tovar.its_marked,tovar.its_excise," +
                        " COALESCE(#t_t2_nick_shop_num_cash.code, #t_t_nick_shop_num_cash.tovar_code) AS code," +
                        " CASE WHEN #t_t2_nick_shop_num_cash.personal_price IS NOT NULL THEN " +
                        " #t_t2_nick_shop_num_cash.personal_price " +
                        " ELSE " +
                        " ISNULL(#t_t_nick_shop_num_cash.prices_by_price_type,0) " +
                        " END AS price  INTO #t_t3_nick_shop_num_cash " +
                        " FROM  tovar LEFT JOIN #t_t_nick_shop_num_cash ON tovar.code =  #t_t_nick_shop_num_cash.tovar_code LEFT JOIN #t_t2_nick_shop_num_cash ON tovar.code =  #t_t2_nick_shop_num_cash.code " +
                        " WHERE #t_t_nick_shop_num_cash.prices_by_price_type IS NOT NULL  OR #t_t2_nick_shop_num_cash.personal_price IS NOT NULL OR tovar.datetime_update>'"+ queryPacketData.LastDateDownloadTovar + "';" +
                        " IF OBJECT_ID('tempdb..#t_t_nick_shop_num_cash') IS NOT NULL  DROP TABLE #t_t_nick_shop_num_cash;" +
                        " IF OBJECT_ID('tempdb..#t_t2_nick_shop_num_cash') IS NOT NULL  DROP TABLE #t_t2_nick_shop_num_cash;" +
                        " CREATE CLUSTERED INDEX code_index ON #t_t3_nick_shop_num_cash (code ASC);";

                    query = query.Replace("_nick_shop_num_cash", nick_shop + "_" + queryPacketData.NumCash);
                    query = query.Replace("@nick_shop", "'" + nick_shop + "'");

                    SqlCommand command = new SqlCommand(query, conn);
                    SqlDataReader reader = command.ExecuteReader();
                    loadPacketData.ListTovar = new List<Tovar>();
                    while (reader.Read())
                    {
                        using (Tovar tovar = new Tovar())
                        {                            
                            tovar.Code = reader["code"].ToString();
                            tovar.Name = reader["name"].ToString();
                            tovar.RetailPrice = reader["price"].ToString().Replace(",", ".");
                            tovar.ItsDeleted = reader["its_deleted"].ToString();
                            tovar.Nds = reader["nds"].ToString();
                            tovar.ItsCertificate = reader["its_certificate"].ToString();
                            tovar.PercentBonus = reader["percent_bonus"].ToString().Replace(",", ".");
                            tovar.TnVed = reader["tnved"].ToString();
                            tovar.ItsMarked = reader["its_marked"].ToString();
                            tovar.ItsExcise = (Convert.ToBoolean(reader["its_excise"]) == false ? "0" : "1");
                            loadPacketData.ListTovar.Add(tovar);
                        }
                    }
                    reader.Close();

                    //query = " SELECT barcode.tovar_code, barcode.barcode  FROM barcode WHERE barcode.tovar_code in(" +
                    //        " SELECT nabor.code FROM (SELECT tovar.code FROM tovar  " +
                    //        " LEFT JOIN prices  ON  tovar.code = prices.tovar_code  AND shop = '" + nick_shop + "'   AND prices.characteristic IS NULL  WHERE tovar.datetime_update>'" + queryPacketData.LastDateDownloadTovar + "')AS nabor)";
                    query = " SELECT barcode.tovar_code, barcode.barcode  FROM barcode WHERE barcode.tovar_code in(" +
                           " SELECT #t_t3_nick_shop_num_cash.code FROM #t_t3_nick_shop_num_cash) AND barcode.datetime_update>'" + queryPacketData.LastDateDownloadTovar + "';";                    
                    query = query.Replace("_nick_shop_num_cash", nick_shop + "_" + queryPacketData.NumCash);

                    command = new SqlCommand(query, conn);
                    reader = command.ExecuteReader();
                    loadPacketData.ListBarcode = new List<Barcode>();
                    while (reader.Read())
                    {
                        using (Barcode barcode = new Barcode())
                        {
                            barcode.TovarCode = reader["tovar_code"].ToString();
                            barcode.BarCode = reader["barcode"].ToString();
                            loadPacketData.ListBarcode.Add(barcode);
                        }
                    }
                    reader.Close();

                    query = " SELECT date_start,date_end,action_active.num_doc,tip,barcode,persent,sum,comment " +
                            " ,mark,disc_only,time_start,time_end " +
                            " ,bonus_promotion,with_old_promotion,day_mon,day_tue" +
                            " ,day_wed,day_thu,day_fri,day_sat,day_sun,promo_code" +
                            " ,sum_bonus,execution_order,kind " +
                            " FROM  (SELECT num_doc FROM action_active where shop='" + nick_shop + "') AS action_active " +
                            " LEFT JOIN action_header ON action_active.num_doc = action_header.num_doc ";

                    command = new SqlCommand(query, conn);
                    reader = command.ExecuteReader();
                    loadPacketData.ListActionHeader = new List<ActionHeader>();
                    while (reader.Read())
                    {
                        using (ActionHeader actionHeader = new ActionHeader())
                        {
                            actionHeader.DateStarted = reader.GetDateTime(0).ToString("yyyy-MM-dd");
                            actionHeader.DateEnd = reader.GetDateTime(1).ToString("yyyy-MM-dd");
                            actionHeader.NumDoc = reader["num_doc"].ToString();
                            actionHeader.Tip = reader["tip"].ToString();
                            actionHeader.Barcode = reader["barcode"].ToString();
                            //actionHeader.CodeTovar = reader["present"].ToString().Replace(",", ".");
                            actionHeader.Persent = reader["persent"].ToString().Replace(",", ".");
                            actionHeader.sum = reader["sum"].ToString().Replace(",", ".");
                            actionHeader.Comment = reader["comment"].ToString().Trim();
                            actionHeader.Marker = reader["mark"].ToString().Trim();
                            actionHeader.ActionByDiscount = reader["disc_only"].ToString().Trim();
                            actionHeader.TimeStart = reader["time_start"].ToString().Trim();
                            actionHeader.TimeEnd = reader["time_end"].ToString().Trim();
                            actionHeader.BonusPromotion = reader["bonus_promotion"].ToString().Trim();
                            actionHeader.WithOldPromotion = reader["with_old_promotion"].ToString().Trim();
                            actionHeader.Monday = reader["day_mon"].ToString().Trim();
                            actionHeader.Tuesday = reader["day_tue"].ToString().Trim();
                            actionHeader.Wednesday = reader["day_wed"].ToString().Trim();
                            actionHeader.Thursday = reader["day_thu"].ToString().Trim();
                            actionHeader.Friday = reader["day_fri"].ToString().Trim();
                            actionHeader.Saturday = reader["day_sat"].ToString().Trim();
                            actionHeader.Sunday = reader["day_sun"].ToString().Trim();
                            actionHeader.PromoCode = reader["promo_code"].ToString().Trim();
                            actionHeader.SumBonus = reader["sum_bonus"].ToString().Trim();
                            actionHeader.ExecutionOrder = reader["execution_order"].ToString();
                            actionHeader.Kind = reader["kind"].ToString(); 

                            loadPacketData.ListActionHeader.Add(actionHeader);
                        }
                    }
                    reader.Close();

                    query = " SELECT action_active.num_doc,action_table.num_list,action_table.tovar,action_table.price " +
                        " FROM  (SELECT num_doc FROM action_active where shop='" + nick_shop + "') AS action_active " +
                        " LEFT JOIN action_table ON action_active.num_doc = action_table.num_doc  WHERE action_table.tovar IN " +
                        "(SELECT tovar.code FROM tovar LEFT JOIN prices  ON  tovar.code = prices.tovar_code " +
                        " where shop = '" + nick_shop + "')";

                    command = new SqlCommand(query, conn);
                    //command.CommandTimeout = 300;
                    reader = command.ExecuteReader();
                    loadPacketData.ListActionTable = new List<ActionTable>();
                    while (reader.Read())
                    {
                        using (ActionTable actionTable = new ActionTable())
                        {
                            actionTable.NumDoc = reader["num_doc"].ToString();
                            actionTable.NumList = reader["num_list"].ToString();
                            actionTable.CodeTovar = reader["tovar"].ToString();
                            actionTable.Price = reader["price"].ToString().Replace(",", ".");
                            loadPacketData.ListActionTable.Add(actionTable);
                        }
                    }
                    reader.Close();

                    query = " SELECT characteristic.tovar_code, characteristic.guid, characteristic.name, prices.price AS retail_price" +
                        " FROM characteristic LEFT JOIN prices ON characteristic.guid = prices.characteristic  where shop = '" + nick_shop + "' AND prices.characteristic is not null " +
                        " GROUP BY characteristic.tovar_code, characteristic.guid, characteristic.name,prices.price";
                    command = new SqlCommand(query, conn);
                    reader = command.ExecuteReader();
                    loadPacketData.ListCharacteristic = new List<Characteristic>();
                    while (reader.Read())
                    {
                        using (Characteristic characteristic = new Characteristic())
                        {
                            characteristic.CodeTovar = reader["tovar_code"].ToString();
                            characteristic.Guid = reader["guid"].ToString();
                            characteristic.Name = reader["name"].ToString();
                            characteristic.RetailPrice = reader["retail_price"].ToString().Replace(",", ".");
                            loadPacketData.ListCharacteristic.Add(characteristic);
                        }
                    }
                    reader.Close();

                    query = "SELECT code,code_tovar,rating,is_active  FROM certificate WHERE datetime_update>'" + queryPacketData.LastDateDownloadTovar + "'";
                    command = new SqlCommand(query, conn);
                    reader = command.ExecuteReader();
                    loadPacketData.ListSertificate = new List<Sertificate>();
                    while (reader.Read())
                    {
                        using (Sertificate sertificate = new Sertificate())
                        {
                            sertificate.Code = reader["code"].ToString();
                            sertificate.CodeTovar = reader["code_tovar"].ToString();
                            sertificate.Rating = reader["rating"].ToString();
                            sertificate.IsActive = reader["is_active"].ToString();
                            loadPacketData.ListSertificate.Add(sertificate);
                        }
                    }
                    reader.Close();

                    query = " SELECT promo_active.code,shop,promo_text.text " +
                        " FROM promo_active  LEFT JOIN promo_text " +
                        " ON promo_active.code = promo_text.code " +
                        " where promo_active.shop = '" + nick_shop + "'" +
                        " order by promo_text.code ";
                    loadPacketData.ListPromoText = new List<PromoText>();
                    command = new SqlCommand(query, conn);
                    reader = command.ExecuteReader();
                    int numstr = 1;
                    while (reader.Read())
                    {
                        using (PromoText promoText = new PromoText())
                        {
                            promoText.AdvertisementText = reader["text"].ToString();
                            promoText.NumStr = numstr.ToString();
                            numstr++;
                            loadPacketData.ListPromoText.Add(promoText);
                        }
                    }
                    reader.Close();

                    query = " SELECT [action_clients].[num_doc],[code_client] FROM [dbo].[action_clients] " +
                            " LEFT JOIN action_active ON action_clients.num_doc=action_active.num_doc AND action_active.shop='" + nick_shop + "'";
                    command = new SqlCommand(query, conn);
                    reader = command.ExecuteReader();
                    loadPacketData.ListActionClients = new List<ActionClients>();
                    while (reader.Read())
                    {
                        using (ActionClients actionClients = new ActionClients())
                        {
                            actionClients.NumDoc = reader["num_doc"].ToString();
                            actionClients.CodeClient = reader["code_client"].ToString();
                            loadPacketData.ListActionClients.Add(actionClients);
                        }
                    }
                    reader.Close();

                   
                    query = "SELECT limit FROM constants";
                    command = new SqlCommand(query, conn);
                    loadPacketData.Threshold = Convert.ToInt32(command.ExecuteScalar());

                    conn.Close();
                    loadPacketData.PacketIsFull = true;
                    if (check_avalible_dataV8(scheme))
                    {
                        loadPacketData.Exchange = true;
                    }
                    string jason = JsonConvert.SerializeObject(loadPacketData, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    string jason_encrypt = CryptorEngine.Encrypt(jason, true, key);
                    result = CompressString(jason_encrypt);
                }
                catch(Exception ex)
                {
                    //return "-1"; здесь ничего не делаем 
                    //loadPacketData.PacketIsFull останется false и так мы поймем , что пакет не полный
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nick_shop"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [WebMethod]
        public bool UploadDataOnSalesPortionJason(string nick_shop, string data, string scheme)
        {

            bool result = false;

            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                return result;
            }
            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + code_shop.Trim();           
            string s = "";
            StringBuilder query_insert_data_on_sales = new StringBuilder();
            string decrypt_data = CryptorEngine.Decrypt(data, true, key);
            SalesPortions salesPortions = JsonConvert.DeserializeObject <SalesPortions>(decrypt_data);
            if (Convert.ToDouble(salesPortions.Version) < 10873126877)
            {
                return result;
            }                

            if ((salesPortions.Shop == nick_shop) && (salesPortions.Guid == code_shop.Trim()))
            {
                foreach (SalesPortionsHeader sph in salesPortions.ListSalesPortionsHeader)
                {                 
                    s = "DELETE FROM sales_header WHERE guid='" + sph.Guid + "';";
                    query_insert_data_on_sales.Append(s);
                    s = "DELETE FROM sales_table WHERE guid='" + sph.Guid + "';";
                    query_insert_data_on_sales.Append(s);
                }              

                StringBuilder sb = new StringBuilder();
                //Соберем запрос из шапок документов
                foreach (SalesPortionsHeader sph in salesPortions.ListSalesPortionsHeader)
                {
                    s = "INSERT INTO sales_header(shop," +
                                                " num_doc," +
                                                "num_cash," +
                                                "client," +
                                                "bonus_counted," +
                                                "discount," +
                                                "sum," +
                                                "check_type," +
                                                "have_action," +
                                                "date_time_start," +
                                                "date_time_write," +
                                                "its_deleted," +
                                                "bonus_writen_off," +
                                                "action," +
                                                "sum_cash," +
                                                "sum_terminal," +
                                                "sum_certificate," +
                                                //"sales_assistant," +
                                                "autor," +
                                                "comment," +
                                                "version," +
                                                "its_print,"+
                                                "transactionId," +
                                                "transactionIdSales," +
                                                "clientInfo_vatin,"+
                                                "clientInfo_name,"+
                                                "sum_cash_remainder,"+
                                                "num_order,"+
                                                "VizaD,"+
                                                "sno,"+
                                                "sum_cash1,"+
                                                "sum_terminal1," +
                                                "sum_certificate1," +
                                                "guid)" +
                                                " VALUES('" + sph.Shop + "'," +
                                                sph.Num_doc + "," +
                                                sph.Num_cash + ",'" +
                                                sph.Client + "'," +
                                                sph.Bonus_counted + "," +
                                                sph.Discount + "," +
                                                sph.Sum + "," +
                                                sph.Check_type + ",'" +
                                                sph.Have_action + "','" +
                                                sph.Date_time_start + "','" +
                                                sph.Date_time_write + "'," +
                                                sph.Its_deleted + "," +
                                                sph.Bonus_writen_off + "," +
                                                sph.Action + "," +
                                                sph.Sum_cash + "," +
                                                sph.Sum_terminal + "," +
                                                sph.Sum_certificate + "," +
                                                //sph.Sales_assistant + "'," +
                                                sph.Autor + ",'" +
                                                sph.Comment + "',"+
                                                salesPortions.Version+","+
                                                sph.Its_print+",'"+
                                                sph.Id_transaction+"','"+
                                                sph.Id_transaction_sale+"','"+
                                                sph.ClientInfo_vatin+"','"+
                                                sph.ClientInfo_name+"',"+
                                                sph.SumCashRemainder+","+
                                                sph.NumOrder+","+
                                                sph.VizaD+","+
                                                sph.SystemTaxation+","+
                                                sph.Sum_cash1+","+
                                                sph.Sum_terminal1+","+
                                                sph.Sum_certificate1+",'"+
                                                sph.Guid+"')";
                    query_insert_data_on_sales.Append(s);
                }
                foreach (SalesPortionsTable spt in salesPortions.ListSalesPortionsTable)
                {
                    s = "INSERT INTO sales_table(shop,"+
                                                    "num_doc,"+
                                                    "num_cash,"+
                                                    "tovar,"+
                                                    "quantity,"+
                                                    "price," +
                                                    " price_d,"+
                                                    "sum,"+
                                                    "sum_d,"+
                                                    "action1,"+
                                                    "action2,"+
                                                    "action3,"+
                                                    "characteristic,"+
                                                    "date_time_write,"+
                                                    "num_str,"+
                                                    "bonus_stand,"+
                                                    "bonus_prom,"+
                                                    "promotion_b_mover,"+
                                                    "marking_code,"+
                                                    "guid)" +
                                            "VALUES('"+spt.Shop+"',"+
                                            spt.Num_doc+","+
                                            spt.Num_cash+","+
                                            spt.Tovar+","+
                                            spt.Quantity+","+
                                            spt.Price+","+
                                            spt.Price_d+","+
                                            spt.Sum+","+
                                            spt.Sum_d+","+
                                            spt.Action1+","+
                                            spt.Action2+","+
                                            spt.Action3+",'"+
                                            spt.Characteristic+"','"+
                                            spt.Date_time_write+"',"+
                                            spt.Num_str+","+
                                            spt.Bonus_stand+","+
                                            spt.Bonus_prom+","+
                                            spt.Promotion_b_mover+",'"+
                                            spt.MarkingCode+"','"+
                                            spt.Guid+"')";
                    query_insert_data_on_sales.Append(s);
                }
                //DateTime dt_start = DateTime.Now;
                result = execute_insert_query(query_insert_data_on_sales.ToString(), 2, scheme);
                //DateTime dt_finish = DateTime.Now;
                //string query = "INSERT INTO stat (shop,date_time_begin ,date_time_end) VALUES " +
                //    "('" + nick_shop + "','" + dt_start.ToString("dd-MM-yyyy HH:mm:ss") + "','" + dt_finish.ToString("dd-MM-yyyy HH:mm:ss") + "')";
                //execute_insert_query(query, 2);
            }         

            return result;
        }
        public class SalesPortions
        {
            public string Version { get; set; }
            public string Shop { get; set; }
            public string Guid { get; set; }
            public List<SalesPortionsHeader> ListSalesPortionsHeader { get; set; }
            public List<SalesPortionsTable> ListSalesPortionsTable { get; set; }
        }
        public class SalesPortionsHeader
        {
            public string Shop { get; set; }
            public string Num_doc { get; set; }
            public string Num_cash { get; set; }
            public string Client { get; set; }
            public string Discount { get; set; }
            public string Sum { get; set; }
            public string Check_type { get; set; }
            public string Have_action { get; set; }
            public string Its_deleted { get; set; }
            public string Bonus_counted { get; set; }
            public string Bonus_writen_off { get; set; }
            public string Date_time_write { get; set; }
            public string Action { get; set; }
            public string Sum_cash { get; set; }
            public string Sum_terminal { get; set; }
            public string Sum_certificate { get; set; }
            public string Sum_cash1 { get; set; }
            public string Sum_terminal1 { get; set; }
            public string Sum_certificate1 { get; set; }
            public string Date_time_start { get; set; }
            //public string Sales_assistant { get; set; }
            public string Comment { get; set; }
            public string Autor { get; set; }
            public string Its_print { get; set; }
            public string Id_transaction { get; set; }
            public string Id_transaction_sale { get; set; }
            public string ClientInfo_vatin { get; set; }
            public string ClientInfo_name { get; set; }
            public string SumCashRemainder { get; set; }
            public string NumOrder { get; set; }
            public string VizaD { get; set; }
            public string SystemTaxation { get; set; }
            public string Guid { get; set; }


        }
        public class SalesPortionsTable
        {
            public string Shop { get; set; }
            public string Num_doc { get; set; }
            public string Num_cash { get; set; }
            public string Tovar { get; set; }
            public string Characteristic { get; set; }
            public string Quantity { get; set; }
            public string Price { get; set; }
            public string Price_d { get; set; }
            public string Sum { get; set; }
            public string Sum_d { get; set; }
            public string Action1 { get; set; }
            public string Action2 { get; set; }
            public string Action3 { get; set; }
            public string Date_time_write { get; set; }
            public string Num_str { get; set; }
            public string Bonus_stand { get; set; }
            public string Bonus_prom { get; set; }
            public string Promotion_b_mover { get; set; }
            public string MarkingCode { get; set; }
            public string Guid { get; set; }
        }
                
        /// <summary>
        /// По нику магазина вернет его уникальный ид
        /// который является частью ключа
        /// </summary>
        /// <param name="num_shop"></param>
        /// <returns></returns>
        private string get_id_database(string num_shop, string scheme)
        {
            string result = "";
                                   
            SqlConnection conn = new SqlConnection(scheme == "1" ? getConnectionString() : getConnectionString2());
            try
            {
                conn.Open();
                string query = "SELECT guid FROM shops WHERE code ='" + num_shop.Trim() + "'";
                SqlCommand command = new SqlCommand(query, conn);
                object rez_query = command.ExecuteScalar();
                if (rez_query != null)
                {
                    result = rez_query.ToString();
                }
                conn.Close();
                command.Dispose();
            }
            catch (SqlException ex)
            {
                //File.AppendAllText("C:\\DistrCashProgram\\Russia\\Test.txt", ex.Message + "\r\n");
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return result;
        }


        public class LoginPassPromo
        {
            public string PassPromoForCashDeskNumber { get; set; }
            public string CashDeskNumber { get; set; }
            public string LoginPromoForCashDeskNumber { get; set; }
        }

        [WebMethod]
        public string GetParametersOnBonusProgram(string nick_shop, string data, string scheme)
        {
            string result = "-1";           

            if (nick_shop.Trim().Length == 0)
            {
                return result;
            }

            //Проверка доступа 
            string code_shop = get_id_database(nick_shop, scheme);
            if (code_shop.Trim().Length == 0)
            {
                return result;
            }

            string count_day = CryptorEngine.get_count_day();
            string key = nick_shop.Trim() + count_day.Trim() + nick_shop.Trim();
            string decrypt_data = CryptorEngine.Decrypt(data, true, key);
            LoginPassPromo passPromo = JsonConvert.DeserializeObject<LoginPassPromo>(decrypt_data);

            SqlConnection conn = new SqlConnection(scheme == "1" ? getConnectionString() : getConnectionString2());
            try
            {
                conn.Open();
                string query = "SELECT login,password FROM cashbox WHERE shop='"+nick_shop+"'  AND num_cash='"+passPromo.CashDeskNumber+"'";
                SqlCommand command = new SqlCommand(query, conn);
                SqlDataReader reader = command.ExecuteReader();
                //string resul_query = "";
                while (reader.Read())
                {
                    //resul_query = reader["password"].ToString().Trim();
                    passPromo.PassPromoForCashDeskNumber = reader["password"].ToString().Trim();
                    passPromo.LoginPromoForCashDeskNumber = reader["login"].ToString().Trim();
                }
                
                //if (resul_query != "")
                //{
                //    passPromo.PassPromoForCashDeskNumber = resul_query;
                //}
                //else
                //{
                //    passPromo.PassPromoForCashDeskNumber = "";
                //}
                command.Dispose();
                conn.Close();
                string jason = JsonConvert.SerializeObject(passPromo, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                result = CryptorEngine.Encrypt(jason, true, key);
            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return result;
        }

    }
}
