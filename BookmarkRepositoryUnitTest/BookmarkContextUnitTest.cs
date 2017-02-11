using System;
using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TagSortService;
using System.Configuration;
using Bookmarks.Common;

namespace BookmarkRepositoryUnitTest
{
    [TestFixture]
    public class BookmarkContextUnitTest
    {
        string connectionString;

        IBookmarksContext context;
        IBookmarksContext Context
        {
            get{ return context;}
        }
        /// <summary>
        /// Loads tags from flat file. Removes quotes, trimming whitespaces and converts everything to lower case        
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IEnumerable<string> LoadTagBundle(string path)
        {
            using (var reader = File.OpenText(path))
            {
                var txt = reader.ReadToEnd();
                return txt.Split(new char[] { '\r', '\n', '\t', ',' }
                               , StringSplitOptions.RemoveEmptyEntries)
                                    .Select(str => str.Replace("\"", "").Trim().ToLower())
                                    .Where(str => !str.Equals(string.Empty));
            }
        }

        [SetUp]
        public void SetupContext()
        {
            string appConfigFilePath =
                @"C:\code\csharp-vs2013\TagSortService\BookmarkRepositoryUnitTest\app.config";

            Configuration config = ConfigurationManager.OpenExeConfiguration
                (ConfigurationUserLevel.None);

            AppDomain.CurrentDomain.SetData
                ("APP_CONFIG_FILE", appConfigFilePath);

            ConnectionStringsSection section =
                config.GetSection("connectionStrings")
                as ConnectionStringsSection;

            connectionString = section.ConnectionStrings[0].ConnectionString;

            if (context == null)
                context = new Bookmarks.Mongo.Data.BookmarksContext(connectionString);
                
        }

        //[TestCase]
        public void TestGetMostFrequentTags()
        {
            var processedTags = Context.CalculateTermCounts
                                        (Bookmarks.Mongo.Data.BookmarksContext.TAG_COUNTS_PAGE_SIZE);

            Assert.IsNotEmpty(processedTags);            
        }
                
        //[TestCase("571db1d1083989dcf1e6e923")]
        public void TestGetNextMostFrequentTags(string tagBundleId)
        {
            var nextMostFreqTags = Context.GetNextMostFrequentTags
                                            (tagBundleId, null
                                           , Bookmarks.Mongo.Data.BookmarksContext.TAG_COUNTS_PAGE_SIZE);

            Assert.IsNotEmpty(nextMostFreqTags);
        }
        
        //[TestCase("mstech,security","571db1d1083989dcf1e6e923")]        
        public void TestExtractExcludeTags(string testBundleNames, string bundleId) 
        {            
            var tagBundle = Context.GetTagBundleById("571db1d1083989dcf1e6e923");

            var excludeTags = Context.ExtractExcludeTags
                                        (testBundleNames.Split
                                                        (new char[]{','}, StringSplitOptions.RemoveEmptyEntries)
                                        , tagBundle);

            Assert.IsNotEmpty(excludeTags);
        }

        //[TestCase("","571db1d1083989dcf1e6e923")]        
        public void TestNothing2Extract(string testBundleNames, string bundleId)
        {            
            var tagBundle = Context.GetTagBundleById("571db1d1083989dcf1e6e923");

            var excludeTags = Context.ExtractExcludeTags
                (testBundleNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                , tagBundle);

            Assert.IsNotEmpty(excludeTags);
        }

        //[TestCase("trading", "57e13f7e84e39a17d49bb198")]
        public void TestCreateTagBundle(string name, string bookmarkCollectionsId)
        {
            Context.CreateTagBundle(TagBundle.Create(name, new string[]{ bookmarkCollectionsId }));
        }
        
        //[TestCase("mstech"
        //        , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\mstech-top-tags.txt"
        //        , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4mstech.txt")]
        //[TestCase("security"
        //        , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\security-top-tags.txt"
        //        , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4security.txt")]
        //[TestCase("android"
        //        , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\mobile-top-tags.txt"
        //        , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4-android.txt")]
        //[TestCase("machine-learning"
        //        , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\ML-top-tags.txt"
        //        , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4-ML.txt")]
        //[TestCase("diy"
        //        , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\diy-top-tags.txt"
        //        , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4-diy.txt")]
        //[TestCase("computer-networks"
        //        , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\computer-networks-top-tags.txt"
        //        , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4-computer-networks.txt")]
        //[TestCase("books"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\books-top-tags.txt"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4-books.txt")]
        //[TestCase("linux"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\linux-top-tags.txt"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4-linux.txt")]
        //[TestCase("cryptocurrencies"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\cryptocurrencies-top-tags.txt"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4-cryptocurrencies.txt")]
        //[TestCase("video"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\video-top-tags.txt"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4-video.txt")]
        //[TestCase("tools", @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\tools-top-tags.txt"
        //        , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4-tools.txt")]  
        //[TestCase("sourcecode"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\sourcecode-top-tags.txt"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4-sourcecode.txt")]
        //[TestCase("communication"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\communication-top-tags.txt"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4-communication.txt")]
        //[TestCase("virtualization"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\virtualization-top-tags.txt"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4-virtualization.txt")]
        //[TestCase("webdev"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\webdev-top-tags.txt"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4-webdev.txt")]
        //[TestCase("moocs"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\moocs-top-tags.txt"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4-moocs.txt")]
        //[TestCase("cryptography", @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\cryptography-top-tags.txt"
        //       , @"C:\code\csharp6\Tagging-Util\solr_import_util\storage\exclude-list4-cryptography.txt")]
        public void TestUpdateTagBundle(string name
                                      , string tagBundleFile
                                      , string excludeFile)
        {

            var tagBundle2Update = new TagBundle
            {
                Name = name
                ,
                Tags = new string[] { }
                ,
                ExcludeTags = new string[] { }
            };

            tagBundle2Update.Tags = LoadTagBundle(tagBundleFile).ToArray();
            tagBundle2Update.ExcludeTags = LoadTagBundle(excludeFile).ToArray();

            Context.UpdateTagBundle(tagBundle2Update);
        }

        //[TestCase("580f053784e39a3724a7ad13", "trading", "test1,test2", "xTest1,xTest2")]
        //[TestCase("571dbbd6083989dcf1e6e92e", "diy", "embedded,robotics,diy,arduino,3dprinting,raspberrypi,openhardware,sensors,instructables,rfid,robots,microcontrollers,repair,exoskeleton", "")]
        public void TestUpdateTagBundleById(string bundleId
                                      , string name  
                                      , string tags
                                      , string exclTags)
        {

            var tagBundle2Update = new TagBundle
            {
                Id = bundleId
                ,
                Name = name
                ,
                Tags = tags.Split(',').ToArray()
                ,
                ExcludeTags = exclTags.Split(',').ToArray()
            };

            Context.UpdateTagBundleById(tagBundle2Update);
        }

        //[TestCase(null)]
        //[TestCase("webdev")]
        public void TestGetTagBundles(string name)
        {
            Assert.IsNotEmpty(Context.GetTagBundles(name));
        }

        //[TestCase("571da189083989dcf1e6e920")]
        //[TestCase("580f053784e39a3724a7ad13")]
        //[TestCase("580f842d84e39a25c4945986")]
        public void TestGetTagBundleById(string objId)
        {
            var bundle = Context.GetTagBundleById(objId);
            Assert.IsNotNull(bundle);
        }

        //[TestCase("mstech")]
        public void TestGetBookmarksByTagBundle(string tagBundleName)
        {
            var bookmarks = Context.GetBookmarksByTagBundle(tagBundleName, null, null);
            Assert.IsNotEmpty(bookmarks);
        }

        //[TestCase("security")]
        public void TestGetBookmarksByTagBundleLimited(string tagBundleName)
        {
            var bookmarks = Context.GetBookmarksByTagBundle(tagBundleName, 10, 50);
            Assert.IsNotEmpty(bookmarks);
        }

        //[TestCase("security")]
        //public void TestGetBookmarksByTagBundleAsync(string tagBundleName)
        //{
        //    var processor = new BookmarksContext(connectionString);
        //    var bookmarksTask = processor.GetBookmarksByTagBundleAsync(tagBundleName, null, null);
        //    Assert.IsNotEmpty(bookmarksTask.Result);
        //}

        //[TestCase("ukatay", "test@test.ca", "6pgMQ16OGh2fMZk4dkkfn0uuY85O4IftT1sIL69B3v4=")]
        //public void TestCreateUser(string userName, string email, string passwordHash)
        //{
        //    ///var sha = SHA256Managed.Create();
        //    //var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes("pwd of uka-horse:)"));//comes from passwd manager            
        //    //var hash = Convert.ToBase64String(bytes);

        //    var processor = new BookmarksContext(connectionString);
        //    var user = new User { Name = userName, Email = email, PasswordHash = passwordHash };
        //    processor.CreateUser(user);
        //}

        //[TestCase("ukatay", "6pgMQ16OGh2fMZk4dkkfn0uuY85O4IftT1sIL69B3v4=")]
        public void TestGetUserByUsernameAndPasswdHash(string userName, string passwordHash)
        {
            var user = Context.GetUserByUsernameAndPasswdHash(userName, passwordHash);
            Assert.IsNotNull(user);
        }

        //[TestCase("bookmarks")]
        public void TestCreateBookmarksCollection(string name)
        {
            Context.CreateBookmarksCollection(name);
        }

        //[TestCase]
        public void TestGetBookmarksCollections()
        {
            Assert.IsNotEmpty(Context.GetBookmarksCollections());
        }

        //[TestCase("571da189083989dcf1e6e920")]
        public void TestGetAssociatedTerms(string tagBundleId)
        {
            var tagBundle = Context.GetTagBundleById(tagBundleId);
            var associatedTags = Context.GetAssociatedTerms(tagBundle, 1500);

            Assert.IsNotEmpty(associatedTags);
        }

        //[TestCase(null)]
        //[TestCase("57e13f7e84e39a17d49bb198")]
        public void TestGetTagBundleNames(string bookmarksCollectionId)
        {
            var bundles = Context.GetTagBundleNames(bookmarksCollectionId);
            Assert.IsNotEmpty(bundles);
            Assert.IsTrue(bundles.Count() > 3);
        }

        
        //[TestCase("581e138784e39a31585db8f9", "57e13f7e84e39a17d49bb198")]
        //[TestCase("5820977a84e39a09805b0282", "57e13f7e84e39a17d49bb198")]
        //[TestCase("580f8c5984e39a3028f4e743", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571dbfec083989dcf1e6e931", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571db334083989dcf1e6e925", "57e13f7e84e39a17d49bb198")]                
        //[TestCase("571dbfec083989dcf1e6e931", "57e13f7e84e39a17d49bb198")]        
        //[TestCase("571db1d1083989dcf1e6e923", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571db10d083989dcf1e6e922", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e920", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571db019083989dcf1e6e921", "57e13f7e84e39a17d49bb198")]  
        //[TestCase("581e082584e39a31585db8f8", "57e13f7e84e39a17d49bb198")]  
        //[TestCase("571db2a3083989dcf1e6e924", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e925", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571db452083989dcf1e6e926", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571db58d083989dcf1e6e927", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571db629083989dcf1e6e928", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571db6d5083989dcf1e6e929", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571db75a083989dcf1e6e92a", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571db82c083989dcf1e6e92d", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571dbbd6083989dcf1e6e92e", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571dbc70083989dcf1e6e92f", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571dbd55083989dcf1e6e930", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571dc0c5083989dcf1e6e932", "57e13f7e84e39a17d49bb198")]
        //[TestCase("580f053784e39a3724a7ad13", "57e13f7e84e39a17d49bb198")]
        //[TestCase("580f842d84e39a25c4945986", "57e13f7e84e39a17d49bb198")]
        public void TestUpdateTagBundleBookmarksCollectionId(string bundleId
                                      , string bookmarkCollectionId)
        {
            Context.UpdateBookmarkCollectionId(bundleId, bookmarkCollectionId);
        }

        //[Test]
        public void TestBackupBookmarks()
        {
            var backup = Context.BackupBookmarks();
            Assert.IsNotEmpty(backup);
        }
    }
}

