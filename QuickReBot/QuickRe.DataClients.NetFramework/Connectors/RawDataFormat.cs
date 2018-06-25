using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickRe.DataClients.NetFramework.Connectors
{
    [Serializable]
    public class RequestDbObject
    {
        public string TypeChoice;
        public string RequestEnvironment;
        public string RequestUrl;
        public string AccessType;
        public string Quantity;
        public string Alias;
        public string Name;
        public string BusinessJustification;
        public string QuantityChoice;
    }

    [Serializable]
    public struct Given
    {
        public string FieldName;
        public string FieldValue;
    };
    [Serializable]
    public class ResponseObject
    {
        public string UserQuery { get; set; }
        public string ToFind { get; set; }
        public string RetreivedValue { get; set; }
        public string ThresholdValue { get; set; }

        public Given GivenValues;
        public DatabaseObject SystemObject { get; set; }

        public string ResponseToUser { get; set; }

        public ResponseObject()
        {
            UserQuery = "";
        }
    }

    public class CombinedBase
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
    }

    public class RawDataFormat
    {
        public string Id { get; set; }
        public string Lob { get; set; }
        public string Description { get; set; }
        public string NumberOfEmployees { get; set; }
        public string FoundedYear { get; set; }
        public string HelpDesk { get; set; }
        public string HomepageUrl { get; set; }
        public string TwitterHandle { get; set; }
        public string PhoneNumber { get; set; }
        public string CreatedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public List<CombinedBase> CombinedBaseSystem { get; set; }
    }
}
