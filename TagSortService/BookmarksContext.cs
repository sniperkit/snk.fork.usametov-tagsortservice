using Bookmarks.Mongo.Data;
using Bookmarks.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace TagSortService
{
    public class BookmarksContext : IBookmarksContext 
    {
        public const string DEFAULT_BOOKMARKS_COLLECTION = "bookmarks";
        public const string BOOKMARKS_COLLECTIONS = "bookmarks_collections";
        public const string USERS_COLLECTION = "users";
        private const string DEFAULT_BOOKMARK_DB = "astanova-bookmarks";
        public const int TAG_COUNTS_PAGE_SIZE = 1000;
        private const string TAG_BUNDLES_COLLECTION = "tagBundles";

        public string BookmarksCollection { get; set; }

        // This is ok... Normally, we put into an IoC container.
        IMongoClient _client;
        IMongoDatabase _database;
        
        IMapper MapperObj { get; set; }
        
        public BookmarksContext(string connectionString, string bookmarksCollection = DEFAULT_BOOKMARKS_COLLECTION)
        {

            ConnectionString = connectionString;
            _client = new MongoClient(ConnectionString);
            _database = _client.GetDatabase(DEFAULT_BOOKMARK_DB);
            BookmarksCollection = bookmarksCollection;

            Init();
            ConfigureMappings();
        }

        private static void Init()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(Bookmarks.Mongo.Data.TagBundle)))
                BsonClassMap.RegisterClassMap<Bookmarks.Mongo.Data.TagBundle>(cm =>
                {
                    cm.AutoMap();
                    cm.MapCreator(t =>
                    new Bookmarks.Mongo.Data.TagBundle
                    {
                        Id = t.Id
                        ,
                        ExcludeTags = t.ExcludeTags
                        ,
                        Name = t.Name
                        ,
                        Tags = t.Tags
                    });
                    cm.SetIgnoreExtraElements(true);
                });
        }

        public void ConfigureMappings() {

            var config = new MapperConfiguration(cfg => 
            {
                cfg.CreateMap<string, Bookmarks.Common.TagCount>().ForMember(dest => dest.Tag, opt => opt.MapFrom(s => s));
                cfg.CreateMap<Bookmarks.Common.TagCount, string>().ConvertUsing(src => src.Tag);

                cfg.CreateMap<Bookmarks.Mongo.Data.TagBundle, Bookmarks.Common.TagBundle>();                    

                cfg.CreateMap<Bookmarks.Common.TagBundle, Bookmarks.Mongo.Data.TagBundle>();

                cfg.CreateMap<Bookmarks.Mongo.Data.BookmarksCollections, Bookmarks.Common.BookmarksCollections>();
                cfg.CreateMap<Bookmarks.Mongo.Data.TagCount, Bookmarks.Common.TagCount>();
                cfg.CreateMap<Bookmarks.Mongo.Data.Bookmark, Bookmarks.Common.Bookmark>();
                cfg.CreateMap<Bookmarks.Mongo.Data.User, Bookmarks.Common.User>();
            });                            

            MapperObj = config.CreateMapper();       
            
        }

        public string ConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Calculate term counts
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Bookmarks.Common.TagCount> CalculateTermCounts(int bufferSize = TAG_COUNTS_PAGE_SIZE)
        {
            var bookmarks = _database.GetCollection<BsonDocument>(BookmarksCollection);
            var aggregate = bookmarks.Aggregate(BuildTagCountsPipelineDefinition(0, bufferSize));

            return aggregate.ToList().Select(tc => MapperObj.Map<Bookmarks.Common.TagCount>(tc)).ToList();
        }


        /// <summary>
        /// builds pipeline definitions
        /// TODO: try using memoization here
        /// </summary>
        /// <param name="skipNum"></param>
        /// <param name="take">TODO: this should be controlled by providing proper buffer size</param>
        /// <returns></returns>
        private PipelineDefinition<BsonDocument, Bookmarks.Mongo.Data.TagCount> BuildTagCountsPipelineDefinition(int skipNum, int take)
        {
            var projectTags = new BsonDocument
            {
                {
                    "$project", new BsonDocument
                    {
                        {"_id", 0},
                        { "Tags", 1 }
                    }
                }
            };

            var unwindTags = new BsonDocument
            {
                {
                    "$unwind", "$Tags"
                }
            };

            var groupByTag = new BsonDocument
            {
                {"$group",
                    new BsonDocument
                    {
                        {"_id",  new BsonDocument
                                             {
                                                 {
                                                     "Tag","$Tags"
                                                 }
                                             }
                        },
                        {"Count", new BsonDocument
                                            {
                                                {
                                                    "$sum", 1
                                                }
                                            }
                        }
                    }
                }
            };

            var projectRename = new BsonDocument
            {
                {
                    "$project", new BsonDocument
                    {
                        {"_id", 0 },
                        {"Tag", "$_id.Tag"},
                        { "Count", 1 }
                    }
                }
            };

            var sort = new BsonDocument
            {
                {"$sort",
                    new BsonDocument { { "Count", -1 } }
                }
            };

            var skip = new BsonDocument
            {
                {
                    "$skip", skipNum
                }
            };

            var limit = new BsonDocument
            {
                {
                    "$limit", take
                }
            };

            return new[]
            { projectTags, unwindTags, groupByTag
            , projectRename, sort, skip, limit };

        }

        public IEnumerable<Bookmarks.Common.TagCount> GetAssociatedTerms
                                            (Bookmarks.Common.TagBundle tagBundle, int bufferSize)
        {
            var bookmarks = _database.GetCollection<BsonDocument>(BookmarksCollection);            

            var filteredBookmarks = GetBookmarksByTagBundle(tagBundle, (int?)0, (int?)bufferSize);

            var tagCounts = filteredBookmarks.SelectMany(b => b.Tags)
                                    .Where(t => !tagBundle.Tags.Contains(t)
                                             && !tagBundle.ExcludeTags.Contains(t))
                                    .Select(t => new Bookmarks.Common.TagCount { Tag = t, Count = 1 });

            return tagCounts.GroupBy(tc => tc.Tag)
                            .Select(tagGrp
                                        => new Bookmarks.Common.TagCount 
                                        {
                                            Tag = tagGrp.Key, Count = tagGrp.Count() 
                                        })
                            .OrderByDescending(tc=>tc.Count);
        }        
        /// <summary>
        /// get tags associations
        /// </summary>
        /// <param name="inclTags"></param>
        /// <param name="skipNum"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        private PipelineDefinition<BsonDocument, Bookmarks.Mongo.Data.TagCount> BuildAssociatedTagsPipelineDefinition(string[] inclTags, int skipNum, int take)
        {
            //db.bookmarks.aggregate([ {$match:{'Tags': {$elemMatch: {$in: ['jobsearch','jobinterview']}}}} ])
            var matchTags = new BsonDocument 
            {
                {
                    "$match", new BsonDocument
                    {
                        {
                            "Tags", new BsonDocument
                            {
                                { 
                                    "$elemMatch", new BsonDocument
                                    {
                                        { "$in", string.Format("['{0}']", string.Join("','", inclTags)) }                                        
                                    }
                                }
                            }
                        } 
                    }
                }
            };

            var projectTags = new BsonDocument
            {
                {
                    "$project", new BsonDocument
                    {
                        {"_id", 0},
                        { "Tags", 1 }
                    }
                }
            };

            var unwindTags = new BsonDocument
            {
                {
                    "$unwind", "$Tags"
                }
            };

            var groupByTag = new BsonDocument
            {
                {"$group",
                    new BsonDocument
                    {
                        {"_id",  new BsonDocument
                                             {
                                                 {
                                                     "Tag","$Tags"
                                                 }
                                             }
                        },
                        {"Count", new BsonDocument
                                            {
                                                {
                                                    "$sum", 1
                                                }
                                            }
                        }
                    }
                }
            };

            var projectRename = new BsonDocument
            {
                {
                    "$project", new BsonDocument
                    {
                        {"_id", 0 },
                        {"Tag", "$_id.Tag"},
                        { "Count", 1 }
                    }
                }
            };

            var sort = new BsonDocument
            {
                {"$sort",
                    new BsonDocument { { "Count", -1 } }
                }
            };

            var skip = new BsonDocument
            {
                {
                    "$skip", skipNum
                }
            };

            var limit = new BsonDocument
            {
                {
                    "$limit", take
                }
            };

            return new[]
            { projectTags, unwindTags, groupByTag
            , projectRename, sort, skip, limit };

        }


        public void CreateTagBundle(Bookmarks.Common.TagBundle tagBundle)
        {
            if (tagBundle == null)
                throw new ArgumentNullException("tagBundle");

            var tagBundles = _database.GetCollection
                <Bookmarks.Mongo.Data.TagBundle>(TAG_BUNDLES_COLLECTION);

            if (tagBundles == null)
                throw new ArgumentNullException("tagBundleS collection");

            tagBundles.InsertOne(MapperObj.Map<Bookmarks.Mongo.Data.TagBundle>(tagBundle));
        }

        public void UpdateTagBundle(Bookmarks.Common.TagBundle tagBundle)
        {
            var tagBundles = _database.GetCollection<Bookmarks.Mongo.Data.TagBundle>(TAG_BUNDLES_COLLECTION);
            var builder = Builders<Bookmarks.Mongo.Data.TagBundle>.Filter;
            var filter = builder.Eq(t => t.Name, tagBundle.Name);
            var update = Builders<Bookmarks.Mongo.Data.TagBundle>.Update
                .Set(t => t.Tags, tagBundle.Tags)
                .Set(t => t.ExcludeTags, tagBundle.ExcludeTags)
                .CurrentDate("lastModified");

            tagBundles.UpdateOne(filter, update);
        }

        public void UpdateTagBundleById(Bookmarks.Common.TagBundle tagBundle)
        {
            var tagBundles = _database.GetCollection
                <Bookmarks.Mongo.Data.TagBundle>(TAG_BUNDLES_COLLECTION);

            var builder = Builders<Bookmarks.Mongo.Data.TagBundle>.Filter;
            var filter = builder.Eq(t => t.Id, tagBundle.Id);
            var update = Builders<Bookmarks.Mongo.Data.TagBundle>.Update
                //.Set(t => t.Name, tagBundle.Name)//no ui for this
                .Set(t => t.Tags, tagBundle.Tags)
                .Set(t => t.ExcludeTags, tagBundle.ExcludeTags)                
                .CurrentDate("lastModified");

            tagBundles.UpdateOne(filter, update);
        }

        /// <summary>
        /// gets tag bundle(s)
        /// </summary>
        /// <param name="name">if this is null or empty then get all</param>
        /// <returns></returns>
        public IEnumerable<Bookmarks.Common.TagBundle> GetTagBundles(string name)
        {
            IEnumerable<Bookmarks.Mongo.Data.TagBundle> result = null;
            var tagBundles = _database.GetCollection<Bookmarks.Mongo.Data.TagBundle>(TAG_BUNDLES_COLLECTION);

            if (string.IsNullOrEmpty(name))
            {
                result = tagBundles.Find(new BsonDocument()).ToList();
            }
            else
            {
                var builder = Builders<Bookmarks.Mongo.Data.TagBundle>.Filter;
                var filter = builder.Eq(t => t.Name, name);
                result = tagBundles.Find(filter).ToList();
            }

            return result.Select(tb => MapperObj.Map<Bookmarks.Common.TagBundle>(tb));
        }

        public Bookmarks.Common.TagBundle GetTagBundleById(string objId)
        {
            if (string.IsNullOrEmpty(objId))
                throw new ArgumentNullException("objId");

            var tagBundles = _database.GetCollection
                <Bookmarks.Mongo.Data.TagBundle>(TAG_BUNDLES_COLLECTION);

            var builder = Builders<Bookmarks.Mongo.Data.TagBundle>.Filter;
            var filter = builder.Eq(t => t.Id, objId);

            var resultTask = tagBundles.Find(filter).FirstAsync();

            var bundle = MapperObj.Map<Bookmarks.Common.TagBundle>(resultTask.Result);            
            
            //bundle.Tags = CleanupTagCounts(bundle.Tags);

            //bundle.ExcludeTags = CleanupTagCounts(bundle.ExcludeTags);

            return bundle;
        }

        private Bookmarks.Common.TagCount[] CleanupTagCounts(Bookmarks.Common.TagCount[] tagCounts)
        {
            var tags = tagCounts.Distinct().ToArray();
            foreach (var tc in tags)
            {
                tc.Count = -1;
            }

            return tags;
        }

        public IEnumerable<Bookmarks.Common.TagCount> GetNextMostFrequentTags(string tagBundleId, string[] excludeTagBundles, int limitTermCounts = TAG_COUNTS_PAGE_SIZE)
        {
            var tagCounts = CalculateTermCounts(limitTermCounts);
            //get tag bundle by name
            var tagBundle = GetTagBundleById(tagBundleId);

            if (tagBundle == null)
                throw new ApplicationException("tagBundle not found");

            IEnumerable<string> excludedTags = ExtractExcludeTags(excludeTagBundles ?? new string[0], tagBundle);

            var filteredTags = tagCounts.Where(tc => !excludedTags.Contains(tc.Tag))
                                        .OrderByDescending(tc => tc.Count).ToList();

            return filteredTags;
        }

        internal IEnumerable<string> ExtractExcludeTags(string[] excludeTagBundles, Bookmarks.Common.TagBundle tagBundle)
        {
            var exclTags = excludeTagBundles.SelectMany(ext => GetTagBundles(ext))
                                           .SelectMany(b=>b.Tags).ToArray();

            string[] excludeTags = tagBundle == null ? new string[0] : tagBundle.ExcludeTags;
            string[] tags        = tagBundle == null ? new string[0] : tagBundle.Tags;

            return exclTags.Union(excludeTags)
                           .Union(tags);
        }


        public IEnumerable<Bookmarks.Common.Bookmark> GetBookmarksByTagBundle
            (string tagBundleName, int? skip, int? take)
        {
            //get tag bundle by name
            var tagBundle = GetTagBundles(tagBundleName).FirstOrDefault();

            if (tagBundle == null)
                throw new ApplicationException("tagBundle not found");
            //should be in tagBundle.Tags
            //HACK!
            var filterDef = string.Format("{{ 'Tags': {{$elemMatch: {{$in: ['{0}'] }} }} }}"
                , string.Join("','", tagBundle.Tags));

            return FilterBookmarks(filterDef, skip, take).ToList()
                .Select(bm => MapperObj.Map<Bookmarks.Common.Bookmark>(bm));
        }

        public IEnumerable<Bookmarks.Common.Bookmark> BackupBookmarks()
        {
            var bookmarks = _database.GetCollection<Bookmarks.Mongo.Data.Bookmark>(BookmarksCollection);            
            return bookmarks.Find(new BsonDocument()).ToList()
                .Select(bm => MapperObj.Map<Bookmarks.Common.Bookmark>(bm));
        }

        public IEnumerable<Bookmarks.Common.Bookmark> GetBookmarksByTagBundle
           (Bookmarks.Common.TagBundle tagBundle, int? skip, int? take)
        {
            if (tagBundle == null)
                throw new ApplicationException("tagBundle not found");
            //should be in tagBundle.Tags
            //HACK!
            var filterDef = string.Format("{{ 'Tags': {{$elemMatch: {{$in: ['{0}'] }} }} }}"
                , string.Join("','", tagBundle.Tags));

            return FilterBookmarks(filterDef, skip, take).ToList()
                .Select(bm => MapperObj.Map<Bookmarks.Common.Bookmark>(bm));
        }
                

        private IFindFluent<Bookmarks.Mongo.Data.Bookmark, Bookmarks.Mongo.Data.Bookmark> FilterBookmarks(string filterDef, int? skip, int? take)
        {
            var bookmarks = _database.GetCollection<Bookmarks.Mongo.Data.Bookmark>(BookmarksCollection);
            return bookmarks.Find(filterDef).Skip(skip).Limit(take);
        }

        public Bookmarks.Common.User GetUserByUsernameAndPasswdHash(string userName, string passwordHash)
        {

            var users = _database.GetCollection<Bookmarks.Mongo.Data.User>(USERS_COLLECTION);

            return MapperObj.Map<Bookmarks.Common.User>
                (users.Find(u => u.Name == userName && u.PasswordHash == passwordHash).FirstOrDefault());
        }

        public IEnumerable<Bookmarks.Common.BookmarksCollections> GetBookmarksCollections()
        {
            var bookmarksCollections = _database.GetCollection
                <Bookmarks.Mongo.Data.BookmarksCollections>(BOOKMARKS_COLLECTIONS);

            return bookmarksCollections.Find(new BsonDocument()).ToList()
                .Select(bmc => MapperObj.Map<Bookmarks.Common.BookmarksCollections>(bmc));
        }

        public void CreateBookmarksCollection(string name)
        {
            var bookmarksCollections = _database.GetCollection<Bookmarks.Mongo.Data.BookmarksCollections>(BOOKMARKS_COLLECTIONS);

            if (bookmarksCollections == null)
                throw new ArgumentNullException("bookmarksCollections");

            bookmarksCollections.InsertOne(new Bookmarks.Mongo.Data.BookmarksCollections { Name = name });
        }

        //public void CreateUser(User user)
        //{
        //    var users = _database.GetCollection<User>(USERS_COLLECTION);
        //    users.InsertOne(user);            
        //}


        public void UpdateExcludeList(string tagBundleId, string[] excludeTags)
        {
            var tagBundles = _database.GetCollection
                <Bookmarks.Mongo.Data.TagBundle>(TAG_BUNDLES_COLLECTION);

            var builder = Builders<Bookmarks.Mongo.Data.TagBundle>.Filter;
            var filter = builder.Eq(t => t.Id, tagBundleId);
            var update = Builders<Bookmarks.Mongo.Data.TagBundle>.Update
                .Set(t => t.ExcludeTags, excludeTags)
                .CurrentDate("lastModified");

            tagBundles.UpdateOne(filter, update);
        }

        public void UpdateBookmarkCollectionId(string tagBundleId, string bookmarksCollectionId)
        {
            var tagBundles = _database.GetCollection
                <Bookmarks.Mongo.Data.TagBundle>(TAG_BUNDLES_COLLECTION);

            var builder = Builders<Bookmarks.Mongo.Data.TagBundle>.Filter;
            var filter = builder.Eq(t => t.Id, tagBundleId);
            var update = Builders<Bookmarks.Mongo.Data.TagBundle>.Update
                .Set(t => t.BookmarksCollectionId, bookmarksCollectionId)
                .CurrentDate("lastModified");

            tagBundles.UpdateOne(filter, update);
        }


        public IEnumerable<Bookmarks.Common.TagBundle> GetTagBundleNames(string bookmarksCollectionId)
        {
            var tagBundles = _database.GetCollection
                <Bookmarks.Mongo.Data.TagBundle>(TAG_BUNDLES_COLLECTION);

            IFindFluent<Bookmarks.Mongo.Data.TagBundle, Bookmarks.Mongo.Data.TagBundle> query;

            if (string.IsNullOrEmpty(bookmarksCollectionId))
                query = tagBundles.Find<Bookmarks.Mongo.Data.TagBundle>(new BsonDocument());            
            else
                query = tagBundles.Find<Bookmarks.Mongo.Data.TagBundle>
                    (t => t.BookmarksCollectionId.Equals(bookmarksCollectionId));

            var result = query.Project(Builders<Bookmarks.Mongo.Data.TagBundle>
                         .Projection
                         .Include("_id")
                         .Include("Name")).ToList();

            return result.Select(t=>
                new Bookmarks.Common.TagBundle
                    { Id = t.GetValue("_id").ToString()
                    , Name = t.GetValue("Name").AsString
                    , BookmarkCollectionId = bookmarksCollectionId}).ToList();
        }

        /// <summary>
        /// this is run-once method, it will be commented out soon
        /// </summary>
        /// <param name="tagBundleId"></param>
        /// <param name="bookmarkCollectionId"></param>
        //public void UpdateTagBundleBookmarkCollectionId(string tagBundleId, string bookmarkCollectionId)
        //{
        //    var tagBundles = _database.GetCollection
        //        <Bookmarks.Mongo.Data.TagBundle>(TAG_BUNDLES_COLLECTION);

        //    var builder = Builders<Bookmarks.Mongo.Data.TagBundle>.Filter;
        //    var filter = builder.Eq(t => t.Id, tagBundleId);
        //    var update = Builders<Bookmarks.Mongo.Data.TagBundle>.Update
        //        .Set(t => t.BookmarksCollectionId, bookmarkCollectionId)
        //        .CurrentDate("lastModified");

        //    tagBundles.UpdateOne(filter, update);
        //}



        public void EditTagBundle(string tagBundleId, string tagBundleName)
        {
            var tagBundles = _database.GetCollection
                <Bookmarks.Mongo.Data.TagBundle>(TAG_BUNDLES_COLLECTION);

            var builder = Builders<Bookmarks.Mongo.Data.TagBundle>.Filter;
            var filter = builder.Eq(t => t.Id, tagBundleId);
            var update = Builders<Bookmarks.Mongo.Data.TagBundle>.Update
                .Set(t => t.Name, tagBundleName)                
                .CurrentDate("lastModified");

            tagBundles.UpdateOne(filter, update);
        }


        public IEnumerable<Bookmarks.Common.TagCount> CalculateRemainingTermCounts(int bufferSize, string[] excludeTagBundles)
        {
            if (excludeTagBundles == null)
                throw new ArgumentNullException("excludeTagBundles");

            var bookmarks = _database.GetCollection<BsonDocument>(BookmarksCollection);
            var aggregate = bookmarks.Aggregate(BuildTagCountsPipelineDefinition(0, bufferSize));

            var tagCounts = aggregate.ToList().Select(tc => MapperObj.Map<Bookmarks.Common.TagCount>(tc)).ToList();

            IEnumerable<string> excludedTags = ExtractExcludeTags(excludeTagBundles, null);

            var filteredTags = tagCounts.Where(tc => !excludedTags.Contains(tc.Tag))
                                        .OrderByDescending(tc => tc.Count).ToList();

            return filteredTags;
        }
    }
}
