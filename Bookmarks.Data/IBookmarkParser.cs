using System.Collections.Generic;

namespace Bookmarks.Data
{
    public interface IBookmarkParser
    {
        List<IBookmark> ParseBookmarks(string filePath);
    }
}
