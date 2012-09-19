using System.Web;
using System.Web.Mvc;

namespace Judgmentrac
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            
            // needed for authentication
            //filters.Add(new AuthorizeAttribute());
            
            // to force SSL for logging in so the authentication ticket the browser passes on all subsequent requests to the web server are secured
            //filters.Add(new RequireHttpsAttribute());
        }
    }
}