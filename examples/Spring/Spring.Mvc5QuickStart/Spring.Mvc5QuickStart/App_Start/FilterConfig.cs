using System.Web;
using System.Web.Mvc;

namespace Spring.Mvc5QuickStart
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}