using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace QuickRe.DataClients.Connectors
{

    public struct Given
    {
        public string FieldName;
        public string FieldValue;
    };

    public class AzureDataClient
    {
        static bool DatabaseUpdated = false;
        static string SearchServiceName = "quickre-searchservice";
        static string AdminApiKey = "CFBA1007524EB70A67E65F56764FECE8";
        static string QueryApiKey = "F53D24B19B7C7C2D2BEA5BE90E9F2E0A";

        public static void InitializeDB()
        {
            SearchServiceClient serviceClient = CreateSearchServiceClient();

            Console.WriteLine("{0}", "Deleting index...\n");
            DeleteIndexIfExists(serviceClient);

            Console.WriteLine("{0}", "Creating index...\n");
            CreateIndex(serviceClient);

            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient("databaseobject");
            Console.WriteLine("{0}", "Uploading documents...\n");
            UploadDocuments(indexClient);


        }

        private static SearchServiceClient CreateSearchServiceClient()
        {
            SearchServiceClient serviceClient = new SearchServiceClient(SearchServiceName, new SearchCredentials(AdminApiKey));
            return serviceClient;
        }
        private static SearchIndexClient CreateSearchIndexClient()
        {
            SearchIndexClient indexClient = new SearchIndexClient(SearchServiceName, "databaseobject", new SearchCredentials(QueryApiKey));
            return indexClient;
        }
        private static void DeleteIndexIfExists(SearchServiceClient serviceClient)
        {
            if (serviceClient.Indexes.Exists("databaseobject"))
            {
                serviceClient.Indexes.Delete("databaseobject");
            }
        }
        private static void CreateIndex(SearchServiceClient serviceClient)
        {
            var definition = new Index()
            {
                Name = "databaseobject",
                Fields = FieldBuilder.BuildForType<DatabaseObject>()
            };

            serviceClient.Indexes.Create(definition);
        }

        private static void UploadDocuments(ISearchIndexClient indexClient)
        {
            String value = File.ReadAllText(@"KnowledgeBaseRaw.json");
            RawDataFormat[] DatabaseObjects = JsonConvert.DeserializeObject<RawDataFormat[]>(value);
            List<DatabaseObject> AzureObjects = new List<DatabaseObject>();


            foreach (RawDataFormat kbobject in DatabaseObjects)
            {
                DatabaseObject newAzureObject = new DatabaseObject();
                newAzureObject.CreatedBy = string.Format("{0}==", kbobject.CreatedBy);
                newAzureObject.CreatedDateTime = string.Format("{0}==", kbobject.CreatedDateTime);
                newAzureObject.Description = string.Format("{0}==", kbobject.Description);
                newAzureObject.FoundedYear = string.Format("{0}==", kbobject.FoundedYear);
                newAzureObject.HelpDesk = string.Format("{0}==", kbobject.HelpDesk);
                newAzureObject.HomepageUrl = string.Format("{0}==", kbobject.HomepageUrl);
                newAzureObject.Id = string.Format("{0}==", kbobject.Id);
                newAzureObject.Lob = string.Format("{0}==", kbobject.Lob);
                newAzureObject.NumberOfEmployees = string.Format("{0}==", kbobject.NumberOfEmployees);
                newAzureObject.PhoneNumber = string.Format("{0}==", kbobject.PhoneNumber);
                newAzureObject.TwitterHandle = string.Format("{0}==", kbobject.TwitterHandle);
                newAzureObject.CombinedBase = new List<string>();

                foreach (CombinedBase combinedBaseObject in kbobject.CombinedBaseSystem)
                {
                    newAzureObject.CombinedBase.Add(combinedBaseObject.Name + ":" + combinedBaseObject.Value + ":" + combinedBaseObject.ValueType + "==");

                }
                AzureObjects.Add(newAzureObject);
            }

            var batch = IndexBatch.MergeOrUpload(AzureObjects);
            try
            {
                indexClient.Documents.Index(batch);
                Console.WriteLine("Indexed");
            }

            catch (IndexBatchException ex)
            {
            }
            Thread.Sleep(2000);
        }

        private static void WriteDocuments(DocumentSearchResult<DatabaseObject> searchResults)
        {
            foreach (SearchResult<DatabaseObject> result in searchResults.Results)
            {
                Console.WriteLine(result.Document);
                List<string> AttributeList = result.Document.CombinedBase;

                foreach (string Name in AttributeList)
                {

                }
            }

            Console.WriteLine();
        }

        //Search
        public static string GetAzureResponse(string Query)
        {
            if (DatabaseUpdated == false)
            {
                InitializeDB();
                DatabaseUpdated = true;
            }

            SearchIndexClient indexClient = new SearchIndexClient(SearchServiceName, "databaseobject", new SearchCredentials(QueryApiKey));


            Given given;
            given.FieldName = "";
            given.FieldValue = "";
            string ToFind = "";

            ToFind = Query.Split(' ').Skip(2).FirstOrDefault();
            given.FieldName = Query.Split(' ').Skip(3).FirstOrDefault();
            given.FieldValue = Query.Split(' ').Skip(4).FirstOrDefault();

            int i, count = 0;
            for (i = 0; i < Query.Length; i++)
            {
                if (Query[i] == ' ')
                    count++;
                if (count == 3)
                    break;
            }
            if (Query.Length - i - 1 > 0)
                Query = Query.Substring(i + 1, Query.Length - i - 1);
            else
                Query = "";


            SearchParameters parameters = new SearchParameters()
            {
                IncludeTotalResultCount = true,
                Select = new[] { "*" },
                Top = 1
            };

            DocumentSearchResult<DatabaseObject> QueryOutputObject = indexClient.Documents.Search<DatabaseObject>(Query, parameters);
            String QueryOutput = "";
            foreach (SearchResult<DatabaseObject> result in QueryOutputObject.Results)
            {
                //QueryOutput += " " + result.Document;
                QueryOutput += " " + ExtractAnswer(result.Document, given, ToFind);
                break;
            }
            return QueryOutput;
        }

        public static string ExtractAnswer(DatabaseObject Object, Given given, string ToFind)
        {
            List<string> AttributeList = Object.CombinedBase;
            AttributeList.Add("CreatedBy:" + Object.CreatedBy);
            AttributeList.Add("CreatedDateTime:" + Object.CreatedDateTime);
            AttributeList.Add("Description:" + Object.Description);
            AttributeList.Add("FoundedYear:" + Object.FoundedYear);
            AttributeList.Add("HelpDesk:" + Object.HelpDesk);
            AttributeList.Add("HomepageUrl:" + Object.HomepageUrl);
            AttributeList.Add("Id:" + Object.Id);
            AttributeList.Add("Lob:" + Object.Lob);
            AttributeList.Add("NumberOfEmployees:" + Object.NumberOfEmployees);
            AttributeList.Add("PhoneNumber:" + Object.PhoneNumber);
            AttributeList.Add("TwitterHandle:" + Object.TwitterHandle);
            string Sofar = "";
            string Sofarans = "";
            string Output = "";

            foreach (string Attribute in AttributeList)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                int from = 0;
                int end = Attribute.Length;

                while (true)
                {
                    end = Attribute.IndexOf(':', from);

                    if (end == -1) break;

                    Sofar = Attribute.Substring(from, end - from);

                    from = end + 1;
                    end = Attribute.IndexOf('=', from);
                    Sofarans = Attribute.Substring(from, end - from);
                    dict[Sofar] = Sofarans;

                    from = end + 1;
                }

                if (dict.ContainsKey(ToFind))
                {
                    if (dict.Count == 1)
                        Output += dict[ToFind] + "\n";
                    else
                    {
                        try
                        {
                            if (string.Equals(given.FieldValue, dict[given.FieldName], StringComparison.OrdinalIgnoreCase))
                            {

                                Output += dict[ToFind] + "\n";
                            }
                        }
                        
                        catch (Exception ex)
                        {

                        }
                    }

                }

            }

            if (Output == "")
                Output = "Sorry. Required Information cannot be retreived. Please rephrase the question.";
            return Output;
        }
    }
}
