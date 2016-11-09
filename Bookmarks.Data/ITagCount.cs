using System;
namespace Bookmarks.Data
{
    public interface ITagCount
    {
        int Count { get; set; }
        string Tag { get; set; }
    }
}
