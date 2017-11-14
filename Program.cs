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

namespace IOTDBSyncer
{
    class Program
    {
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

        public static void insertIntoSQLDB(Entry e)
        {
            string conn = "Persist Security Info=False;Integrated Security=true;Initial Catalog=Northwind;server=DESKTOP-QAE2U1L\\SQLEXPRESS";
            using (SqlConnection openCon = new SqlConnection(conn))
            {
                string saveStaff = "INSERT into [IOTTrans].dbo.Hoja1$ (entry_id, fecha, medida, unidad, numero, modelo, descripcion) VALUES (@id, @fecha, @medida, @unidad, @numero,@modelo, @descripcion)";

                using (SqlCommand querySaveStaff = new SqlCommand(saveStaff))
                {
                    querySaveStaff.Connection = openCon;
                    querySaveStaff.Parameters.AddWithValue("@id", (e.entry_id));
                    querySaveStaff.Parameters.AddWithValue("@fecha",(e.created_at));
                    querySaveStaff.Parameters.AddWithValue("@medida", (e.field1));
                    querySaveStaff.Parameters.AddWithValue("@unidad", "celcius");
                    querySaveStaff.Parameters.AddWithValue("@numero", "206");
                    querySaveStaff.Parameters.AddWithValue("@modelo", "chido");
                    querySaveStaff.Parameters.AddWithValue("@descripcion", "temperatura");
                    openCon.Open();
                    try
                    {
                        querySaveStaff.ExecuteNonQuery();
                    }
                    catch
                    {
                        openCon.Close();
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            string url = "https://api.thingspeak.com/channels/338016/feeds.json?days=1";
            HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create(url);
            getRequest.Method = "GET";
            getRequest.Credentials = new NetworkCredential("UN", "PW");
            ServicePointManager.ServerCertificateValidationCallback = new
               RemoteCertificateValidationCallback
               (
                  delegate { return true; }
               );

            var getResponse = (HttpWebResponse)getRequest.GetResponse();
            Stream newStream = getResponse.GetResponseStream();
            StreamReader sr = new StreamReader(newStream);
            var result = sr.ReadToEnd();
            var splashInfo = JsonConvert.DeserializeObject<RootElement>(result);

            RootElement e = (RootElement)splashInfo;

            foreach (Entry en in e.feeds)
            {
                insertIntoSQLDB(en);
            }

            Console.ReadLine();
        }
    }
}
