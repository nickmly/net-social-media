using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Social
{
    public class NetConfig
    {
        public static HttpClient client = new HttpClient();

        public static void RunClient()
        {
            client.BaseAddress = new Uri("https://www.reddit.com");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<JObject> GetJSONAsync(string path)
        {
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return JObject.Parse(data);
            }
            string json = @"{error: 'Invalid get request'}";
            return JObject.Parse(json);
        }

        public static async Task<JArray> GetJSONArrayAsync(string path)
        {
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return JArray.Parse(data);
            }
            string json = @"{error: 'Invalid get request'}";
            return JArray.Parse(json);
        }
    }
}