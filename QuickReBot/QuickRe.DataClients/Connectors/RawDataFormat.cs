using System;
using System.Collections.Generic;
using System.Text;

namespace QuickRe.DataClients.Connectors
{

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
