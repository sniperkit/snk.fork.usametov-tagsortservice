using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Configuration;
using TagSortService.Models;
using System.Threading.Tasks;
using System.ServiceModel.Activation;

namespace TagSortService
{
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class BookmarkCollectionRepository : IBookmarkCollectionRepository
    {
        IBookmarksContext context;
        IBookmarksContext Context {
            get 
            { 
                if(context==null)
                    context = new BookmarksContext(ConnectionString);

                return context;
            }
        }


        public IEnumerable<BookmarksCollections> GetBookmarkCollections()
        {
            return Context.GetBookmarksCollections();
        }
        
        public IEnumerable<TagCount> CalculateTermCounts(int bufferSize)
        {
            return bufferSize > 0 
                ? Context.CalculateTermCounts(bufferSize) 
                : Context.CalculateTermCounts(BookmarksContext.TAG_COUNTS_PAGE_SIZE);            
        }
        
        public void CreateBookmarksCollection(string name)
        {        
            Context.CreateBookmarksCollection(name);
        }


        public void CreateTagBundle(TagBundle tagBundle)
        {            
            Context.CreateTagBundle(tagBundle);
        }

        public IEnumerable<Bookmark> GetBookmarksByTagBundle(string tagBundleName, int skip, int take)
        {
            return Context.GetBookmarksByTagBundle
                (tagBundleName, skip, take);
        }

        public IEnumerable<Bookmark> GetBookmarksByTagBundle(string tagBundleName, int? skip, int? take)
        {
            return Context.GetBookmarksByTagBundle(tagBundleName, skip, take);
        }
                
        public IEnumerable<TagCount> GetNextMostFrequentTags
            (string tagBundleId, string excludeTagBundleNames, int limitTermCounts)
        {
            string[] excludeTagBundles = new string[0];
            if (!string.IsNullOrEmpty(excludeTagBundleNames))
                excludeTagBundles = excludeTagBundleNames.Split
                    (new char[]{',', '\n','\r'}, StringSplitOptions.RemoveEmptyEntries);

            return Context.GetNextMostFrequentTags
                (tagBundleId, excludeTagBundles, limitTermCounts);
        }

        public TagBundle GetTagBundleById(string objId)
        {
            return Context.GetTagBundleById(objId);
        }

        //public IEnumerable<TagBundle> GetTagBundles(string name, string bookmarksCollectionId)
        //{no need
        //    return Context.GetTagBundles(name); 
        //}
                
        //public void UpdateTagBundle(TagBundle tagBundle)
        //{
        //    Context.UpdateTagBundle(tagBundle);
        //}

        public void UpdateTagBundleById(TagBundle tagBundle)
        {
            Context.UpdateTagBundleById(tagBundle);
        }

        public void UpdateExcludeList(TagBundle tagBundle)
        {
            var exclTags = tagBundle.ExcludeTags.ToStringArray();
            Context.UpdateExcludeList(tagBundle.Id, exclTags);
        }

        public string ConnectionString { 
            get {
                return Utils.GetConnectionString();
            }
        }
        
        public IEnumerable<TagCount> GetAssociatedTerms(string objId, int bufferSize)
        {
            var tagBundle = Context.GetTagBundleById(objId);
            return Context.GetAssociatedTerms(tagBundle, bufferSize);
        }


        public IEnumerable<TagBundle> GetTagBundleNames(string bookmarksCollectionId)
        {
            return Context.GetTagBundleNames(bookmarksCollectionId);
        }
        
        public void UpdateTagBundleNameById(TagBundle tagBundle)
        {
            Context.EditTagBundle(tagBundle.Id, tagBundle.Name);
        }


        public IEnumerable<TagCount> GetRemainingTermCounts(int bufferSize, string excludeTagBundleNames)
        {
            string[] excludeTagBundles = new string[0];
            if (!string.IsNullOrEmpty(excludeTagBundleNames))
                excludeTagBundles = excludeTagBundleNames.Split
                    (new char[]{',', '\n','\r'}, StringSplitOptions.RemoveEmptyEntries);

            return bufferSize > 0
                ? Context.CalculateRemainingTermCounts(bufferSize, excludeTagBundles)
                : Context.CalculateRemainingTermCounts(BookmarksContext.TAG_COUNTS_PAGE_SIZE, excludeTagBundles);            
        }
    }
}
