using System.Runtime.Serialization;
namespace TagSortService.Models
{
    [DataContract]
    public class TagCount 
    {
        [DataMember]
        public int Count
        {
            get;
            set;
        }

        [DataMember]
        public string Tag
        {
            get;
            set;
        }
    }
}