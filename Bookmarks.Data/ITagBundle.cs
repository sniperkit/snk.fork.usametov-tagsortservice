using System;
namespace Bookmarks.Data
{
    public interface ITagBundle
    {
        string[] ExcludeTags { get; set; }
        string Id { get; set; }
        string Name { get; set; }
        string[] Tags { get; set; }
    }
}
