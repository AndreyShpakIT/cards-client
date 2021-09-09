using AppClient.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AppClient
{
    class WebAPI
    {
        private static int timeout = 3000;

        public static string ServerName = "localhost:44351";
        public static readonly string CardsUri = "/cards";
        public static readonly string DeleteUri = "/delete";

        public static Task<HttpResponseMessage> GetCall()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string apiUrl = CreateUri() + CardsUri;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.Timeout = TimeSpan.FromSeconds(900);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.GetAsync(apiUrl);
                response.Wait(timeout);
                return response;
            }
        }
        public static async Task<HttpResponseMessage> PostCall(SCard model)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string apiUrl = CreateUri() + CardsUri;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.Timeout = TimeSpan.FromSeconds(6000);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                JObject json = JObject.FromObject(model);
                string jsonString = json.ToString();
                HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, content);

                return response;
            }
        }
        public static async Task<HttpResponseMessage> PutCall(SCard model)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string apiUrl = CreateUri() + CardsUri;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.Timeout = TimeSpan.FromSeconds(6000);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/png"));

                JObject json = JObject.FromObject(model);
                string jsonString = json.ToString();
                HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var response = await client.PutAsync(apiUrl, content);

                return response;
            }
        }
        public static Task<HttpResponseMessage> DeleteCall(string id)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string apiUrl = CreateUri() + CardsUri + '/' + id;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.Timeout = TimeSpan.FromSeconds(900);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.DeleteAsync(apiUrl);
                response.Wait(timeout);
                return response;
            }
        }
        public static Task<HttpResponseMessage> DeleteCall(long[] ids)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string apiUrl = CreateUri() + CardsUri + DeleteUri;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.Timeout = TimeSpan.FromSeconds(900);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                JArray json = JArray.FromObject(ids);
                string jsonString = json.ToString();
                HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                var response = client.PostAsync(apiUrl, content);
                response.Wait(timeout);
                return response;
            }
        }

        private static string CreateUri()
        {
            return "https://" + ServerName + "/api";
        }
    }
}
