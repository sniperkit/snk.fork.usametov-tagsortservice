using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Configuration;
using System.Threading.Tasks;
using System.ServiceModel.Activation;
using TagSortService.ViewModels;

namespace TagSortService
{
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class BookmarkCollectionRepository : IBookmarkCollectionRepository
    {
        Bookmarks.Common.IBookmarksContext context;
        Bookmarks.Common.IBookmarksContext Context
        {
            get 
            { 
                if(context==null)
                    context = new Bookmarks.Mongo.Data.BookmarksContext(ConnectionString);

                return context;
            }
        }


        public IEnumerable<BookmarksCollections> GetBookmarkCollections()
        {
            return Context.GetBookmarksCollections().Select
                (bc => new BookmarksCollections
                {
                    Id = bc.Id
                    ,
                    Name = bc.Name
                });
        }
        
        public IEnumerable<TagCount> CalculateTermCounts(int bufferSize)
        {
            return bufferSize > 0
                ? MapTagCounts(Context.CalculateTermCounts(bufferSize))
                : MapTagCounts(Context.CalculateTermCounts(Bookmarks.Mongo.Data.BookmarksContext.TAG_COUNTS_PAGE_SIZE));                
        }

        public IEnumerable<TagCount> MapTagCounts(IEnumerable<Bookmarks.Common.TagCount> tagCounts)
        {
            return tagCounts.Select
                        (
                            tc => new TagCount
                            {
                                Count = tc.Count
                                ,
                                Tag = tc.Tag
                            }
                        );
        }

        public void CreateBookmarksCollection(string name)
        {        
            Context.CreateBookmarksCollection(name);
        }


        public void CreateTagBundle(TagBundle tagBundle)
        {
            Context.CreateTagBundle(new Bookmarks.Common.TagBundle
            {
                Name = tagBundle.Name
                ,
                Tags = tagBundle.Tags.Select(t => t.Tag).ToArray(),
                ExcludeTags = tagBundle.ExcludeTags.Select(t => t.Tag).ToArray()
            });
        }

        public IEnumerable<Bookmark> GetBookmarksByTagBundle(string tagBundleName, int skip, int take)
        {
            return MapBookmarks(Context.GetBookmarksByTagBundle(tagBundleName, skip, take));
        }

        private IEnumerable<Bookmark> MapBookmarks(IEnumerable<Bookmarks.Common.Bookmark> bookmarks)
        {
            return bookmarks.Select(b => new Bookmark
                {
                    Id = b.Id
                    ,
                    LinkUrl = b.LinkUrl
                    ,
                    LinkText = b.LinkText
                    ,
                    Description = b.Description
                    ,
                    Tags = b.Tags
                    ,
                    AddDate = b.AddDate
                });
        }

        public IEnumerable<Bookmark> GetBookmarksByTagBundle(string tagBundleName, int? skip, int? take)
        {
            return MapBookmarks(Context.GetBookmarksByTagBundle(tagBundleName, skip, take));
        }
                
        public IEnumerable<TagCount> GetNextMostFrequentTags
            (string tagBundleId, string excludeTagBundleNames, int limitTermCounts)
        {
            string[] excludeTagBundles = new string[0];
            if (!string.IsNullOrEmpty(excludeTagBundleNames))
                excludeTagBundles = excludeTagBundleNames.Split
                    (new char[]{',', '\n','\r'}, StringSplitOptions.RemoveEmptyEntries);

            return MapTagCounts(Context.GetNextMostFrequentTags(tagBundleId, excludeTagBundles, limitTermCounts));
        }

        public ViewModels.TagBundle GetTagBundleById(string objId)
        {
            if (string.IsNullOrEmpty(objId) || objId.Equals("undefined"))
                throw new ArgumentNullException("objId");

            var bundle = Context.GetTagBundleById(objId);

            return new ViewModels.TagBundle
                                    {
                                        Name = bundle.Name
                                        ,
                                        Id = bundle.Id
                                        ,
                                        Tags = MapTags(bundle.Tags)
                                        ,
                                        ExcludeTags = MapTags(bundle.ExcludeTags)
                                        ,
                                        ExcludeTagBundles = bundle.ExcludeTagBundles
                                    };
        }

        private TagSortService.ViewModels.TagCount[] MapTags(string[] tags)
        {
            return tags.Select(t => new TagSortService.ViewModels.TagCount { Tag = t, Count = -1 }).ToArray();
        }

        public void UpdateTagBundleById(TagBundle tagBundle)
        {
            Context.UpdateTagBundleById(
                new Bookmarks.Common.TagBundle
            {
                Id = tagBundle.Id
                ,
                Name = tagBundle.Name
                ,
                Tags = tagBundle.Tags.Select(t => t.Tag).ToArray()
                ,
                ExcludeTags = tagBundle.ExcludeTags.Select(t => t.Tag).ToArray()
                ,
                ExcludeTagBundles = tagBundle.ExcludeTagBundles
            });
        }

        public void UpdateExcludeList(TagBundle tagBundle)
        {            
            Context.UpdateExcludeList(tagBundle.Id, tagBundle.ExcludeTags.Select(t=>t.Tag).ToArray());
        }

        public string ConnectionString { 
            get {
                return Utils.GetConnectionString();
            }
        }
        
        public IEnumerable<TagCount> GetAssociatedTerms(string objId, int bufferSize)
        {
            var tagBundle = Context.GetTagBundleById(objId);
            return MapTagCounts(Context.GetAssociatedTerms(tagBundle, bufferSize));
        }

        public IEnumerable<TagBundle> GetTagBundleNames(string bookmarksCollectionId)
        {
            return Context.GetTagBundleNames(bookmarksCollectionId).Select(tb => new TagBundle
            {
                Id = tb.Id
                ,
                Name = tb.Name                
            });
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
                ? MapTagCounts(Context.CalculateRemainingTermCounts(bufferSize, excludeTagBundles))
                : MapTagCounts(Context.CalculateRemainingTermCounts
                                        (Bookmarks.Mongo.Data.BookmarksContext.TAG_COUNTS_PAGE_SIZE
                                       , excludeTagBundles));            
        }
        
        public IEnumerable<Bookmark> ExportBookmarks()
        {
            return MapBookmarks(Context.BackupBookmarks());
        }


        public void UpdateExcludeTagBundlesList(TagBundle tagBundle)
        {
            Context.UpdateExcludeTagBundlesList(tagBundle.Id, tagBundle.ExcludeTagBundles);
        }
    }
}
