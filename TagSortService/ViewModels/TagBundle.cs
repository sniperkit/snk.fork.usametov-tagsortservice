using System.Runtime.Serialization;

namespace TagSortService.ViewModels
{
    [DataContract]
    public class TagBundle 
    {
        [DataMember]
        public TagCount[] ExcludeTags
        {
            get;
            set;
        }

        [DataMember]
        public string[] ExcludeTagBundles
        {
            get;
            set;
        }

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

        [DataMember]
        public TagCount[] Tags
        {
            get;
            set;
        }

        //[DataMember]
        //public string BookmarksCollectionId
        //{
        //    get;
        //    set;
        //}
    }
}