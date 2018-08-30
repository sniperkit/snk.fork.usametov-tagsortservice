/*
Sniperkit-Bot
- Status: analyzed
*/

﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Runtime.Serialization;


namespace Bookmarks.Mongo.Data
{    
    public class TagBundle 
    {
        [BsonRepresentation(BsonType.ObjectId)]        
        public string Id { get; set; }
                
        public string[] Tags { get; set; }

        public string[] ExcludeTags { get; set; }

        public string[] ExcludeTagBundles { get; set; }
                
        public string Name { get; set; }
                
        public string[] BookmarksCollections { get; set; }
    }
}
