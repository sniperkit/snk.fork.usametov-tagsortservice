/*
Sniperkit-Bot
- Status: analyzed
*/

﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Runtime.Serialization;

namespace Bookmarks.Mongo.Data
{
    public class User 
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }
    }
}
