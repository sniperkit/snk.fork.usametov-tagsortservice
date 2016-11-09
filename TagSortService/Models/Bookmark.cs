using System.Runtime.Serialization;

namespace TagSortService.Models
{
    [DataContract]
    public class Bookmark 
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string LinkUrl
        {
            get;
            set;
        }

        [DataMember]
        public string LinkText
        {
            get;
            set;
        }

        [DataMember]
        public System.DateTime AddDate
        {
            get;
            set;
        }

        [DataMember]
        public System.Collections.Generic.List<string> Tags
        {
            get;
            set;
        }

        [DataMember]
        public string Description
        {
            get;
            set;
        }
    }
}