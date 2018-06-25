using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuickRe.DataClients.NetFramework.Connectors
{
    

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
                newAzureObject.CreatedBy = string.Format("{0}:Text==", kbobject.CreatedBy);
                newAzureObject.CreatedDateTime = string.Format("{0}:Text==", kbobject.CreatedDateTime);
                newAzureObject.Description = string.Format("{0}:Text==", kbobject.Description);
                newAzureObject.FoundedYear = string.Format("{0}:Text==", kbobject.FoundedYear);
                newAzureObject.HelpDesk = string.Format("{0}:Link==", kbobject.HelpDesk);
                newAzureObject.HomepageUrl = string.Format("{0}:Link==", kbobject.HomepageUrl);
                newAzureObject.Id = string.Format("{0}==", kbobject.Id);
                newAzureObject.Lob = string.Format("{0}==", kbobject.Lob);
                newAzureObject.NumberOfEmployees = string.Format("{0}:Text==", kbobject.NumberOfEmployees);
                newAzureObject.PhoneNumber = string.Format("{0}:Text==", kbobject.PhoneNumber);
                newAzureObject.TwitterHandle = string.Format("{0}:Text==", kbobject.TwitterHandle);
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

        public static DatabaseObject GetSystemSelected(string Query)
        {
            if (DatabaseUpdated == false)
            {
                InitializeDB();
                DatabaseUpdated = true;
            }

            SearchIndexClient indexClient = new SearchIndexClient(SearchServiceName, "databaseobject", new SearchCredentials(QueryApiKey));



            int i, count = 0;
            for (i = 0; i < Query.Length; i++)
            {
                if (Query[i] == ' ')
                    count++;
                if (count == 1)
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

            foreach (SearchResult<DatabaseObject> result in QueryOutputObject.Results)
            {
                return result.Document;

            }
            return null;
        }
        //Search
        public static string GetAzureResponse(ResponseObject responseObject, string FromClass="azuredialog")
        {        
            
            

            if (FromClass != "azuredialog")           
            {
                try
                {
                    responseObject.ToFind = responseObject.UserQuery.Split(' ').Skip(1).FirstOrDefault().ToLower();
                    responseObject.GivenValues.FieldName = responseObject.UserQuery.Split(' ').Skip(2).FirstOrDefault();
                    responseObject.GivenValues.FieldValue = responseObject.UserQuery.Split(' ').Skip(3).FirstOrDefault();
                }
                catch (Exception ex) {
                    if (responseObject.ToFind == null)
                        return "NA";
                }
            }
            
            
            ExtractAnswer(responseObject);
            

            return responseObject.RetreivedValue;
        }

        public static string ExtractAnswer(ResponseObject responseObject, string ToFind="usual")
        {
            List<string> AttributeList = responseObject.SystemObject.CombinedBase;
            AttributeList.Add("CreatedBy:" + responseObject.SystemObject.CreatedBy);
            AttributeList.Add("CreatedDateTime:" + responseObject.SystemObject.CreatedDateTime);
            AttributeList.Add("Description:" + responseObject.SystemObject.Description);
            AttributeList.Add("FoundedYear:" + responseObject.SystemObject.FoundedYear);
            AttributeList.Add("HelpDesk:" + responseObject.SystemObject.HelpDesk);
            AttributeList.Add("HomepageUrl:" + responseObject.SystemObject.HomepageUrl);
            AttributeList.Add("Id:" + responseObject.SystemObject.Id);
            AttributeList.Add("Lob:" + responseObject.SystemObject.Lob);
            AttributeList.Add("NumberOfEmployees:" + responseObject.SystemObject.NumberOfEmployees);
            AttributeList.Add("PhoneNumber:" + responseObject.SystemObject.PhoneNumber);
            AttributeList.Add("TwitterHandle:" + responseObject.SystemObject.TwitterHandle);
            string SoFarField = "";
            string Sofarans = "";
            string Output = "";

            if (ToFind == "threshold") ToFind = responseObject.ToFind + "threshold";
            else ToFind = responseObject.ToFind;

            foreach (string Attribute in AttributeList)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                int from = 0;
                int end = Attribute.Length-1;

                while (true)
                {
                    end = Attribute.IndexOf(':', from)-1;
                    if (end <0) break;
                    SoFarField = Attribute.Substring(from, end-from +1);
                    from = end + 2;
                    end = Attribute.IndexOf('=', from)-1;
                    Sofarans = Attribute.Substring(from, end - from+1);
                    dict[SoFarField.ToLower()] = Sofarans;
                    from = end + 1;
                }

                if (dict.ContainsKey(ToFind))
                {
                    if (dict.Count == 1)
                        Output += dict[ToFind];
                    else
                    {
                        try
                        {
                            if (string.Equals(responseObject.GivenValues.FieldValue, dict[responseObject.GivenValues.FieldName], StringComparison.OrdinalIgnoreCase))
                            {

                                Output += dict[ToFind];
                            }
                        }

                        catch (Exception ex)
                        {

                        }
                    }

                }

            }

            if (Output == "")
            {
                Output = "NA";
                return "NA";
            }


            string type = "";
            string OutputToSend = "";
            int Length = Output.Length;
            int TypeStart = Output.LastIndexOf(':');
            if (TypeStart > 0)
            {
                type = Output.Substring(TypeStart + 1, Length - TypeStart - 1);

                if (type.ToLower().Equals("text"))
                {
                    if (ToFind == responseObject.ToFind + "threshold")
                    {
                        responseObject.ThresholdValue = Output;
                        int ValuePosition;
                        string FormattedLob = responseObject.SystemObject.Lob.Substring(0, responseObject.SystemObject.Lob.Length - 2);
                        string FormattedToFind = responseObject.ToFind;
                        ValuePosition = responseObject.ThresholdValue.IndexOf(':');
                        string FormattedThresholdValue = responseObject.ThresholdValue.Substring(0, ValuePosition);
                        OutputToSend = "The Threshold value of " + FormattedToFind + " from " + FormattedLob + " is " + FormattedThresholdValue;
                    }
                        
                    else
                        responseObject.RetreivedValue = Output;

                    
                }
                else
                {
                    
                    if (ToFind == responseObject.ToFind + "threshold")
                    {
                        responseObject.ThresholdValue = "10 hours:text";
                        OutputToSend = "Calling API : " + Output.Substring(0, Output.Length - 5);
                        //Output = "Calling API : " + responseObject.ThresholdValue.Substring(0, responseObject.ThresholdValue.Length - 5) + "\n";
                    }
                        
                    else
                    {
                        responseObject.RetreivedValue = "10 hours:text";
                        OutputToSend = "Calling API : " + Output.Substring(0, Output.Length - 5);
                        //Output = "Calling API : " + responseObject.RetreivedValue.Substring(0, responseObject.RetreivedValue.Length - 5) + "\n";

                    }
                    //OutputToSend += "The " + ToFind + " of " + Object.Lob.Substring(0, Object.Lob.Length - 2) + " is " + Output.Substring(0, Output.Length - 5);
                }
            }

            
            return OutputToSend;
        }
        



        
    }
}
