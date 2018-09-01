using Sms.WebAdmin.Filter;
using System.Web;
using System.Web.Mvc;

namespace Sms.WebAdmin
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ExceptionLogAttribute());
        }
    }
}
