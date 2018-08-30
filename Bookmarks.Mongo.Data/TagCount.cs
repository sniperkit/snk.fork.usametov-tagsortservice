/*
Sniperkit-Bot
- Status: analyzed
*/

﻿using System.Runtime.Serialization;

namespace Bookmarks.Mongo.Data
{
    public class TagCount 
    {
        public string Tag{ get; set; }

        public int Count { get; set; }
    }
}
