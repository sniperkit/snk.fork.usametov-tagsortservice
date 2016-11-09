﻿using System.Text.RegularExpressions;

namespace Bookmarks.Data
{
    public static class Utils
    {
        static Regex regex = new Regex(@"[-!_\s]+");

        public static string SanitizeStr(string t)
        {
            return regex.Replace(t, "").ToLowerInvariant();
        }
    }
}
