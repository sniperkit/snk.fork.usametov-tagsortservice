/*
Sniperkit-Bot
- Status: analyzed
*/

﻿using Bookmarks.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Bookmarks.Mongo.Data
{    
    public class Bookmark : IBookmark
    {       
        public string Id { get; set; }
                
        public bool IsPrivate { get; set; }
                
        public DateTime AddDate
        {
            get;
            set;
        }
                
        public string Description
        {
            get;
            set;
        }

        public string LinkText
        {
            get;
            set;
        }
                
        public string LinkUrl
        {
            get;
            set;
        }
                
        public List<string> Tags
        {
            get;
            set;
        }
    }
}
