using Microsoft.Azure.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickRe.DataClients.Connectors
{

    public class DatabaseObject
    {
        [System.ComponentModel.DataAnnotations.Key]
        [IsFilterable]
        public string Id { get; set; }

        [IsFilterable, IsSearchable, IsSortable, IsFacetable]
        public string Lob { get; set; }

        [IsFilterable, IsSearchable, IsSortable, IsFacetable]
        public string Description { get; set; }

        [IsFilterable, IsSearchable, IsSortable, IsFacetable]
        public string NumberOfEmployees { get; set; }

        [IsFilterable, IsSearchable, IsSortable, IsFacetable]
        public string FoundedYear { get; set; }

        [IsFilterable, IsSearchable, IsSortable, IsFacetable]
        public string HelpDesk { get; set; }

        [IsFilterable, IsSearchable, IsSortable, IsFacetable]
        public string HomepageUrl { get; set; }

        [IsFilterable, IsSearchable, IsSortable, IsFacetable]
        public string TwitterHandle { get; set; }

        [IsFilterable, IsSearchable, IsSortable, IsFacetable]
        public string PhoneNumber { get; set; }

        [IsFilterable, IsSearchable, IsSortable, IsFacetable]
        public string CreatedDateTime { get; set; }

        [IsFilterable, IsSearchable, IsSortable, IsFacetable]
        public string CreatedBy { get; set; }

        [IsFilterable, IsSearchable, IsFacetable]
        public List<string> CombinedBase { get; set; }
    }

}
