using DBSyncer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Configuration;

namespace IOTDBSyncer
{
    class Program
    {
        private static string last_updated;
        private static readonly HttpClient client = new HttpClient();

        public static async void convertToJson()
        {
            string data = await client.GetStringAsync("https://api.thingspeak.com/channels/338016/feeds.json?days=35");

            Entries e = new JavaScriptSerializer().Deserialize<Entries>(data);

            foreach (var item in e.entries)
            {
                Console.WriteLine(item);
            }
        }

        public static void insertTemp(Entry e, string[] arr) {
            string conn = "Persist Security Info=False;Integrated Security=true;Initial Catalog=;server=DESKTOP-QAE2U1L\\SQLEXPRESS";
            using (SqlConnection openCon = new SqlConnection(conn))
            {
                string saveValue1 = "INSERT into [IOTTrans].dbo.Hoja1$ (entry_id, fecha, medida, unidad, numero, modelo, descripcion) VALUES (@id, @fecha, @medida, @unidad, @numero,@modelo, @descripcion)";
                using (SqlCommand querySave = new SqlCommand(saveValue1))
                {
                    querySave.Connection = openCon;
                    querySave.Parameters.AddWithValue("@id", (e.entry_id));
                    querySave.Parameters.AddWithValue("@fecha", (e.created_at));
                    querySave.Parameters.AddWithValue("@medida", (e.field1));
                    querySave.Parameters.AddWithValue("@unidad", arr[2]);
                    querySave.Parameters.AddWithValue("@numero", arr[1]);
                    querySave.Parameters.AddWithValue("@modelo", arr[3]);
                    querySave.Parameters.AddWithValue("@descripcion", arr[0]);
                    openCon.Open();
                    try
                    {
                        querySave.ExecuteNonQuery();
                    }
                    catch
                    {
                        openCon.Close();
                    }
                    finally
                    {
                        openCon.Close();
                    }
                }
            }
        }
        public static void insertPres(Entry e, string[] arr)
        {
            string conn = "Persist Security Info=False;Integrated Security=true;Initial Catalog=;server=DESKTOP-QAE2U1L\\SQLEXPRESS";
            using (SqlConnection openCon = new SqlConnection(conn))
            {
                string saveValue1 = "INSERT into [IOTTrans].dbo.Hoja1$ (entry_id, fecha, medida, unidad, numero, modelo, descripcion) VALUES (@id, @fecha, @medida, @unidad, @numero,@modelo, @descripcion)";
                using (SqlCommand querySave = new SqlCommand(saveValue1))
                {
                    querySave.Connection = openCon;
                    querySave.Parameters.AddWithValue("@id", (e.entry_id));
                    querySave.Parameters.AddWithValue("@fecha", (e.created_at));
                    querySave.Parameters.AddWithValue("@medida", (e.field2));
                    querySave.Parameters.AddWithValue("@unidad", arr[2]);
                    querySave.Parameters.AddWithValue("@numero", arr[1]);
                    querySave.Parameters.AddWithValue("@modelo", arr[3]);
                    querySave.Parameters.AddWithValue("@descripcion", arr[0]);
                    openCon.Open();
                    try
                    {
                        querySave.ExecuteNonQuery();
                    }
                    catch
                    {
                        openCon.Close();
                    }
                    finally
                    {
                        openCon.Close();
                    }
                }
            }
        }

        public static void insertIntoSQLDB(Entry e, string [] arr1, string [] arr2)
        {
            if (e.field1 == e.field2 && (e.field1 == null || e.field2 == "null" || e.field1 == "" || e.field2.Length == 0))
                return;
            if (e.field1 != null || e.field1 != "null" || e.field1 != "" || e.field1.Length > 0)
                insertTemp(e, arr1);
            if (e.field2 != null || e.field2 != "null" || e.field2 != "" || e.field2.Length > 0)
                insertPres(e, arr2);
        }

        static void updateChannel() {
            string url = "https://api.thingspeak.com/channels/340908/feeds.json?" + ConfigurationSettings.AppSettings["params"];
            HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create(url);
            getRequest.Method = "GET";
            getRequest.Credentials = new NetworkCredential("UN", "PW");
            ServicePointManager.ServerCertificateValidationCallback = new
               RemoteCertificateValidationCallback
               (
                  delegate { return true; }
               );
            Console.WriteLine("Sending Request...");
            var getResponse = (HttpWebResponse)getRequest.GetResponse();
            Stream newStream = getResponse.GetResponseStream();
            StreamReader sr = new StreamReader(newStream);
            var result = sr.ReadToEnd();
            var splashInfo = JsonConvert.DeserializeObject<RootElement>(result);
            RootElement e = (RootElement)splashInfo;
            Console.WriteLine("Information Recieved...");
            string[] arr1 = e.channel.field1.Split(',');
            string[] arr2 = e.channel.field2.Split(',');
            last_updated = e.channel.last_entry_id;
            Console.WriteLine("Syncing Thingspeak with Cube...");
            foreach (Entry en in e.feeds)
            {
                insertIntoSQLDB(en, arr1, arr2);
            }
            Console.WriteLine("Data Synced...");
        }
        public static bool channelSynced() {
            string url = "https://api.thingspeak.com/channels/340908/feeds.json?" + ConfigurationSettings.AppSettings["params"];
            HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create(url);
            getRequest.Method = "GET";
            getRequest.Credentials = new NetworkCredential("UN", "PW");
            ServicePointManager.ServerCertificateValidationCallback = new
               RemoteCertificateValidationCallback
               (
                  delegate { return true; }
               );
            Console.WriteLine("Sending Request...");
            var getResponse = (HttpWebResponse)getRequest.GetResponse();
            Stream newStream = getResponse.GetResponseStream();
            StreamReader sr = new StreamReader(newStream);
            var result = sr.ReadToEnd();
            var splashInfo = JsonConvert.DeserializeObject<RootElement>(result);
            RootElement e = (RootElement)splashInfo;
            return e.channel.last_entry_id == last_updated;
        }
        public static void runSp() {
            string conn = "Persist Security Info=False;Integrated Security=true;Initial Catalog=;server=DESKTOP-QAE2U1L\\SQLEXPRESS";
            using (SqlConnection openCon = new SqlConnection(conn))
            {
                string saveValue1 = "exec [IOT].dbo.[sp_llena_dimim]";
                using (SqlCommand querySave = new SqlCommand(saveValue1))
                {
                    querySave.Connection = openCon;
                    openCon.Open();
                    try
                    {

                        querySave.ExecuteNonQuery();
                    }
                    catch
                    {
                        openCon.Close();
                    }
                    finally
                    {
                        openCon.Close();
                    }
                }
            }
            Console.WriteLine("Cube synced with TS succesfully...");
        }
        static void Main(string[] args)
        {
            last_updated = ConfigurationSettings.AppSettings["last_val"];
            DateTime now;
            while (true) {
                now = DateTime.Now;
                //if (now.Hour == 12 && now.Minute >= 0 && now.Minute <=10)
                //if (now.Minute % 2 == 0 )
                if (now.Minute % 5 == 0 )
                {
                    Console.WriteLine("Starting process...");
                    if (!channelSynced())
                    {
                        Console.WriteLine("Syncing...");
                        updateChannel();
                        runSp();
                        Console.WriteLine("Last updated: " + last_updated);
                    }
                    else {
                        Console.WriteLine("No Need...");
                    }
                    Console.WriteLine("Done!");
                }
            }
            Console.ReadLine();
        }
    }
}
