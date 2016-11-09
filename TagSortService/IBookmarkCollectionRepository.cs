using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using TagSortService.Models;

namespace TagSortService
{
    [ServiceContract]
    public interface IBookmarkCollectionRepository
    {   
        [OperationContract]
        [WebGet(UriTemplate = "/termcounts/?bufferSize={bufferSize}",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        IEnumerable<TagCount> CalculateTermCounts(int bufferSize);

        [OperationContract]
        [WebGet(UriTemplate = "/remaining_termcounts/?bufferSize={bufferSize}&excludeTagBundleNames={excludeTagBundleNames}",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        IEnumerable<TagCount> GetRemainingTermCounts(int bufferSize, string excludeTagBundleNames);

        [OperationContract]
        [WebGet(UriTemplate = "/bookmarkCollections/",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        IEnumerable<BookmarksCollections> GetBookmarkCollections();

        [OperationContract]
        [WebInvoke(UriTemplate = 
            "/bookmarkCollections/create",
            Method="POST",            
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        void CreateBookmarksCollection(string name);

        [OperationContract]
        [WebInvoke(UriTemplate="/tagBundle/create",
            Method="POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        void CreateTagBundle(TagBundle tagBundle);

        [OperationContract]
        [WebGet(UriTemplate = 
            "/BookmarksByTagBundle/?tagBundleName={tagBundleName}&skip={skip}&take={take}",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        IEnumerable<Bookmark> GetBookmarksByTagBundle(string tagBundleName, int skip, int take);
                
        [OperationContract]
        [WebGet(UriTemplate =
            "/tagBundleNames/?bookmarksCollectionId={bookmarksCollectionId}",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]        
        IEnumerable<TagBundle> GetTagBundleNames(string bookmarksCollectionId);

        [OperationContract]
        [WebGet(UriTemplate =
            "/NextMostFrequentTags/?tagBundleId={tagBundleId}&excludeTagBundleNames={excludeTagBundleNames}&limit={limitTermCounts}",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        IEnumerable<TagCount> GetNextMostFrequentTags
            (string tagBundleId, string excludeTagBundleNames, int limitTermCounts);

        [OperationContract]
        [WebGet(UriTemplate =
            "/tagBundle/?id={objId}",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        TagBundle GetTagBundleById(string objId);

                
        //[OperationContract]
        //[WebInvoke(UriTemplate = "/tagBundle/updateByName",
        //    Method = "POST",
        //    BodyStyle = WebMessageBodyStyle.WrappedRequest,
        //    RequestFormat = WebMessageFormat.Json,
        //    ResponseFormat = WebMessageFormat.Json)]
        //void UpdateTagBundle(TagBundle tagBundle);

        [OperationContract]
        [WebInvoke(UriTemplate = "/tagBundle/updateById",
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        void UpdateTagBundleById(TagBundle tagBundle);

        [OperationContract]
        [WebInvoke(UriTemplate = "/tagBundle/editName",
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        void UpdateTagBundleNameById(TagBundle tagBundle);

        [OperationContract]
        [WebInvoke(UriTemplate = "/tagBundle/UpdateExcludeList",
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        void UpdateExcludeList(TagBundle tagBundle);

        [OperationContract]
        [WebInvoke(UriTemplate = "/AssociatedTerms/?tagBundleId={objId}&bufferSize={bufferSize}",
            Method = "GET",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        IEnumerable<TagCount> GetAssociatedTerms(string objId, int bufferSize);
    }           
}
