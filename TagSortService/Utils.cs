using System.Configuration;
using TagSortService.Models;
using System.Linq;

namespace TagSortService
{
    public static class Utils
    {
        public static string GetConnectionString()
        {            
            ConnectionStringsSection section =
                ConfigurationManager.GetSection("connectionStrings") as ConnectionStringsSection;

            if (section.ConnectionStrings.Count > 0)
                return section.ConnectionStrings[0].ConnectionString;
            
            //appHarbor case: get from appSettings
            return ConfigurationManager.AppSettings["MONGOLAB_URI"] ?? 
                    ConfigurationManager.AppSettings.Get("MONGOHQ_URL");            
        }

        public static string[] ToStringArray(this TagCount[] tagCounts) 
        {
            return tagCounts.Select(t => t.Tag).ToArray();
        }
    }
}