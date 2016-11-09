using System.Runtime.Serialization;
namespace TagSortService.Models
{
    [DataContract]
    public class BookmarksCollections
    {
        [DataMember]
        public string Id
        {
            get;
            set;
        }

        [DataMember]
        public string Name
        {
            get;
            set;
        }
    }
}