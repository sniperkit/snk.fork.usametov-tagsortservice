/*
Sniperkit-Bot
- Status: analyzed
*/

﻿using System.Runtime.Serialization;
namespace TagSortService.ViewModels
{
    [DataContract]
    public class BookmarksCollections
    {
        [DataMember]
        public string Id
        {
            get;
            set;
        }

        [DataMember]
        public string Name
        {
            get;
            set;
        }
    }
}