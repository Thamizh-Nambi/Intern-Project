using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QuickRe.DataClients.NetFramework.Connectors
{

    public class Answer
    {
        public List<string> questions { get; set; }
        public string answer { get; set; }
        public double score { get; set; }
        public int id { get; set; }
        public string source { get; set; }
        public List<object> metadata { get; set; }
    }

    public class QnAResponseObject
    {
        public List<Answer> answers { get; set; }
    }
    public class QnADataClient
    {

        // NOTE: Replace this with a valid host name.
        static string host = "https://quickre-knowledgebase-qna.azurewebsites.net";

        // NOTE: Replace this with a valid endpoint key.
        // This is not your subscription key.
        // To get your endpoint keys, call the GET /endpointkeys method.
        static string endpoint_key = "4336c3ef-2862-4004-97d0-2570b299c1a7";

        // NOTE: Replace this with a valid knowledge base ID.
        // Make sure you have published the knowledge base with the
        // POST /knowledgebases/{knowledge base ID} method.
        static string kb = "ae8391b3-a8b7-4009-8d6c-1aa258ef6e52";

        static string service = "/qnamaker";
        static string method = "/knowledgebases/" + kb + "/generateAnswer/";

        static string question = "";

        async static Task<string> Post(string uri, string body)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                request.Headers.Add("Authorization", "EndpointKey " + endpoint_key);

                var response = await client.SendAsync(request);

                return await response.Content.ReadAsStringAsync();
            }
        }

        public async static Task<QnAResponseObject> GetQnAResponse(string Query)
        {
            question = "{'question': '" + Query + "', 'top': 3 } ";
            var uri = host + service + method;
            Console.WriteLine("Calling " + uri + ".");
            var response = await Post(uri, question);
            QnAResponseObject qnAResponseObject = JsonConvert.DeserializeObject<QnAResponseObject>(response);
            Answer QnAResponseFirstAnswer = qnAResponseObject.answers[0];

            return qnAResponseObject;
            //Console.WriteLine(response);
            //Console.WriteLine("Press any key to continue.");
        }




    }
}
