/*
Sniperkit-Bot
- Status: analyzed
*/

﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
//using Bookmarks.Data;

namespace Bookmarks.Mongo.Data
{
    /// <summary>
    /// maps to bookmarks table
    /// </summary>    
    public class BookmarksCollections //: IBookmarksCollections
    {
        [BsonRepresentation(BsonType.ObjectId)]        
        public string Id { get; set; }
                
        public string Name { get; set; }
    }
}
