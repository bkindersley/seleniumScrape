using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace SeleniumEsperiments
{
    class Program
    {
        static void Main(string[] args)
        {

            string csrfJsonString = GetCSRFJson();

            VFCSRFObj csrfJsonObj = JsonConvert.DeserializeObject<VFCSRFObj>(csrfJsonString, new CSRFConverter());

            Debug.Write("done");

        }

        static async Task Send()
        {
            string url = "https://my.service.nsw.gov.au/MyServiceNSW/apexremote";

            HttpClient client = new HttpClient();

            Dictionary<string, string> reqStrings = new Dictionary<string, string>
            {
                { "createMaDashboardTransaction","{ 'action':'maDashboardCtrl','method':'createMaDashboardTransaction','data':['1172fa7c7be06f6f17525c96d74da5855007cfa4ce9edde7cf4f5df0c367c536',''],'type':'rpc','tid':22,'ctx':{'csrf':'VmpFPSxNakF4Tnkwd05TMHlNbFF5TURveE1Ub3hNUzQ1TWpCYSxXb21JLVVzdUJFY2hKWHlwQ2ZDVFhLLFpEQXpZV1E1','vid':'06690000005lUNp','ns':'','ver':34}}" },
                { "createChangeAddressTransaction","{'action':'RMSTransactionService','method':'createChangeAddressTransaction','data':['1172fa7c7be06f6f17525c96d74da5855007cfa4ce9edde7cf4f5df0c367c536',''],'type':'rpc','tid':24,'ctx':{'csrf':'VmpFPSxNakF4Tnkwd05TMHlNbFF5TURveE1Ub3hNUzQ1TWpaYSx0TjVtTmVXSVFkWUtVMEE1ajJpWWZVLE9EWmtZMlZq','vid':'06690000005lUNp','ns':'','ver':34}}" },
                { "validateLicenseDetails", "{'action':'RMSTransactionService','method':'validateLicenseDetails','data':['###','!!!','$$$','%%%'],'type':'rpc','tid':25,'ctx':{'csrf':'VmpFPSxNakF4Tnkwd05TMHlNbFF5TURveE1Ub3hNUzQ1TWpkYSxNazU3RGtpeVpUaG5rREJ2YnU4WlVmLE5EVmtNVFpr','vid':'06690000005lUNp','ns':'','ver':34}}" }

            };
            
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url)) { Content = new StringContent(reqStrings["createChangeAddressTransaction"], Encoding.UTF8, "application/json") };
            request.Headers.Add("X-User-Agent", "Visualforce-Remoting");
            request.Headers.Add("Origin", "https://my.service.nsw.gov.au");
            request.Headers.Add("Referer","https://my.service.nsw.gov.au/MyServiceNSW/index");
            request.Headers.Add("Host" ,"my.service.nsw.gov.au");

            var resp = await client.SendAsync(request);
            object product;
            List<ApexResultObj> res = null;
            if (resp.IsSuccessStatusCode)
            {
                product = await resp.Content.ReadAsAsync<object>();
                
                res = JsonConvert.DeserializeObject<List<ApexResultObj>>(product.ToString());

                string valueToRecord = res.FirstOrDefault()?.result.statusObject;

                request = new HttpRequestMessage(HttpMethod.Post, new Uri(url)) { Content = new StringContent(reqStrings["validateLicenseDetails"].Replace("###", valueToRecord).Replace("!!!", "Smith").Replace("$$$", "111111").Replace("%%%", "1111111"), Encoding.UTF8, "application/json") };
                request.Headers.Add("X-User-Agent", "Visualforce-Remoting");
                request.Headers.Add("Origin", "https://my.service.nsw.gov.au");
                request.Headers.Add("Referer", "https://my.service.nsw.gov.au/MyServiceNSW/index");
                request.Headers.Add("Host", "my.service.nsw.gov.au");

                resp = await client.SendAsync(request);
                if (resp.IsSuccessStatusCode)
                {
                    product = await resp.Content.ReadAsAsync<object>();
                }
            }

            Debug.WriteLine("done");
        }

        static string GetCSRFJson()
        {
            //HttpClient client = new HttpClient();
            //var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://my.service.nsw.gov.au/MyServiceNSW/index"));
            //var resp = await client.SendAsync(request);
            //object product;
            //if (resp.IsSuccessStatusCode)
            //{
            //    product = await resp.Content.ReadAsAsync<object>();

            //}
            string htmlCode = "";
            string jsonString = "";
            using (WebClient client = new WebClient())
            {
                htmlCode = client.DownloadString("https://my.service.nsw.gov.au/MyServiceNSW/index");
            }

            if (!string.IsNullOrEmpty(htmlCode))
            {
                string pattern = @"\bVisualforce\b(.*);";
                Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                MatchCollection matches = rgx.Matches(htmlCode);

                List<string> matchTexts = new List<string>();
                foreach (Match m in matches)
                {
                    matchTexts.Add(m.Value);
                }

                if (matchTexts.Count() > 1)
                {
                    Debug.WriteLine("regex ambiguity");
                    return jsonString;
                }

                string matchedText = matchTexts.FirstOrDefault();

                int firstIndex = matchedText.IndexOf("{");
                int lastIndex = matchedText.LastIndexOf("}");

                if (firstIndex > -1 && lastIndex > firstIndex)
                {
                    jsonString = matchedText.Substring(firstIndex, lastIndex - firstIndex + 1);
                }
                else
                {
                    Debug.WriteLine("bad bracket matching");
                    return jsonString;
                }

            }
            
            return jsonString;
        }
    }

    class ApexResultObj
    {
        public string action = "RMSTransactionService";
        public ApexResult result;
        public string statusCode;
        public string method = "hasRMSEmail";
        public int tid;
        public string type = "rpc";


    }

    class ApexResult
    {
        public string statusCode;
        public string statusMessage;
        public string statusObject;
    }

    class VFCSRFObj
    {
        public Dictionary<string, string> vf { get; set; }
        public Dictionary<string, Dictionary<string, string>> actions { get; set; }
        public Dictionary<string, string> service { get; set; }
    }

    public class CSRFConverter : JsonConverter
     {
 
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
        
        }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject item = JObject.Load(reader);

                if (item["vf"] != null && item["actions"] != null && item["service"] != null)
                {
                    Dictionary<string, string> vf = item["vf"].ToObject<Dictionary<string, string>>(serializer);
                    Dictionary<string, string> service = item["service"].ToObject<Dictionary<string, string>>(serializer);

                    Dictionary<string, Dictionary<string, string>> actions = new Dictionary<string, Dictionary<string, string>>();

                    JProperty actionProp = item.Properties().Where(p => p.Name == "actions").First();

                    JObject actn = (JObject)actionProp.Value;

                    foreach ( JProperty prop in actn.Properties())
                    {
                        if (prop.Value["ms"] != null)
                        {
                            actions.Add(prop.Name, prop.Value["ms"].ToObject<Dictionary<string, string>>(serializer));
                        }
                    }

                    VFCSRFObj ret = new VFCSRFObj();
                    ret.actions = actions;
                    ret.vf = vf;
                    ret.service = service;

                    return ret;
                }

                // This should not happen. Perhaps better to throw exception at this point?
                return null;
            }

            public override bool CanRead
            {
                get { return false; }
            }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}