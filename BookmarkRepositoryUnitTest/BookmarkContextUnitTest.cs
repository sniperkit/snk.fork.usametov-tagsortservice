using System;
using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TagSortService;
using System.Configuration;
using TagSortService.Models;
//using Bookmarks.Data;


namespace BookmarkRepositoryUnitTest
{
    [TestFixture]
    public class BookmarkContextUnitTest
    {
        string connectionString;

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
        public void SetupConnectionString()
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

        }

        //[TestCase]
        public void TestGetMostFrequentTags()
        {
            var processor = new BookmarksContext(connectionString);

            var processedTags = processor.CalculateTermCounts();

            Assert.IsNotEmpty(processedTags);            
        }
                
        //[TestCase("571db1d1083989dcf1e6e923")]
        public void TestGetNextMostFrequentTags(string tagBundleId)
        {
            var processor = new BookmarksContext(connectionString);

            var nextMostFreqTags = processor.GetNextMostFrequentTags(tagBundleId, null);
            Assert.IsNotEmpty(nextMostFreqTags);
        }
        
        //[TestCase("mstech,security","571db1d1083989dcf1e6e923")]        
        public void TestExtractExcludeTags(string testBundleNames, string bundleId) 
        {
            var processor = new BookmarksContext(connectionString);
            var tagBundle = processor.GetTagBundleById("571db1d1083989dcf1e6e923");

            var excludeTags = processor.ExtractExcludeTags
                (testBundleNames.Split
                                (new char[]{','},StringSplitOptions.RemoveEmptyEntries)
                , tagBundle);

            Assert.IsNotEmpty(excludeTags);
        }

        //[TestCase("","571db1d1083989dcf1e6e923")]        
        public void TestNothing2Extract(string testBundleNames, string bundleId)
        {
            var processor = new BookmarksContext(connectionString);
            var tagBundle = processor.GetTagBundleById("571db1d1083989dcf1e6e923");

            var excludeTags = processor.ExtractExcludeTags
                (testBundleNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                , tagBundle);

            Assert.IsNotEmpty(excludeTags);
        }

        //[TestCase("trading", "57e13f7e84e39a17d49bb198")]
        public void TestCreateTagBundle(string name, string bookmarkCollectionsId)
        {

            var processor = new BookmarksContext(connectionString);
            processor.CreateTagBundle(new TagBundle { Name = name, BookmarksCollectionId = bookmarkCollectionsId });
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
                Tags = new TagCount[] { }
                ,
                ExcludeTags = new TagCount[] { }
            };

            tagBundle2Update.Tags = LoadTagBundle(tagBundleFile).Select(t => new TagCount {Tag = t}).ToArray();
            tagBundle2Update.ExcludeTags = LoadTagBundle(excludeFile).Select(t => new TagCount { Tag = t }).ToArray();

            var processor = new BookmarksContext(connectionString);
            processor.UpdateTagBundle(tagBundle2Update);
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
                Tags = tags.Split(',').Select(t => new TagCount {Tag = t}).ToArray()
                ,
                ExcludeTags = exclTags.Split(',').Select(t => new TagCount { Tag = t }).ToArray()
            };            

            var processor = new BookmarksContext(connectionString);
            processor.UpdateTagBundleById(tagBundle2Update);
        }

        //[TestCase(null)]
        //[TestCase("webdev")]
        public void TestGetTagBundles(string name)
        {

            var processor = new BookmarksContext(connectionString);
            Assert.IsNotEmpty(processor.GetTagBundles(name));
        }

        //[TestCase("571da189083989dcf1e6e920")]
        //[TestCase("580f053784e39a3724a7ad13")]
        //[TestCase("580f842d84e39a25c4945986")]
        public void TestGetTagBundleById(string objId)
        {
            var processor = new BookmarksContext(connectionString);
            var bundle = processor.GetTagBundleById(objId);
            Assert.IsNotNull(bundle);
        }

        //[TestCase("mstech")]
        public void TestGetBookmarksByTagBundle(string tagBundleName)
        {
            var processor = new BookmarksContext(connectionString);
            var bookmarks = processor.GetBookmarksByTagBundle(tagBundleName, null, null);
            Assert.IsNotEmpty(bookmarks);
        }

        //[TestCase("security")]
        public void TestGetBookmarksByTagBundleLimited(string tagBundleName)
        {
            var processor = new BookmarksContext(connectionString);
            var bookmarks = processor.GetBookmarksByTagBundle(tagBundleName, 10, 50);
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
            var processor = new BookmarksContext(connectionString);
            var user = processor.GetUserByUsernameAndPasswdHash(userName, passwordHash);
            Assert.IsNotNull(user);
        }

        //[TestCase("bookmarks")]
        public void TestCreateBookmarksCollection(string name)
        {
            var processor = new BookmarksContext(connectionString);

            processor.CreateBookmarksCollection(name);
        }

        //[TestCase]
        public void TestGetBookmarksCollections()
        {
            var processor = new BookmarksContext(connectionString);

            Assert.IsNotEmpty(processor.GetBookmarksCollections());
        }

        //[TestCase("571da189083989dcf1e6e920")]
        public void TestGetAssociatedTerms(string tagBundleId)
        {
            var processor = new BookmarksContext(connectionString);
            var tagBundle = processor.GetTagBundleById(tagBundleId);
            var associatedTags = processor.GetAssociatedTerms(tagBundle, 1500);

            Assert.IsNotEmpty(associatedTags);
        }

        //[TestCase(null)]
        //[TestCase("57e13f7e84e39a17d49bb198")]
        public void TestGetTagBundleNames(string bookmarksCollectionId)
        {
            var processor = new BookmarksContext(connectionString);
            var bundles = processor.GetTagBundleNames(bookmarksCollectionId);
            Assert.IsNotEmpty(bundles);
            Assert.IsTrue(bundles.Count() > 3);
        }

        //[TestCase("571db1d1083989dcf1e6e923", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e920", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e921", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e922", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e924", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e925", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e926", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e927", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e928", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e929", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e92a", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e92d", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e92e", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e92f", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e931", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e932", "57e13f7e84e39a17d49bb198")]
        //[TestCase("571da189083989dcf1e6e930", "57e13f7e84e39a17d49bb198")]        
        //public void TestUpdateTagBundleBookmarksCollectionId(string bundleId
        //                              , string bookmarkCollectionId)
        //{   
        //    var processor = new BookmarksContext(connectionString);
        //    processor.UpdateTagBundleBookmarkCollectionId(bundleId, bookmarkCollectionId);
        //}

    }
}

