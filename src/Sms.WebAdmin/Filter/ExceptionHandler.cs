using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;

namespace Sms.WebAdmin.Filter
{
    public class ExceptionLogAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            string errorMsg= filterContext.Exception.Message;

            if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
            {
                var accept = filterContext.RequestContext.HttpContext.Request.AcceptTypes;
                if (accept.Contains("application/json"))
                {
                    filterContext.Result = new JsonResult() { Data = new { Success = false, Msg = errorMsg } };
                }
                else
                {
                    filterContext.Result = new JavaScriptResult() { Script = "show_message(false,'" + errorMsg + "',null);" };
                }
                filterContext.ExceptionHandled = true;
            }
            else
            {
                //根据状态码处理
                //int statusCode = new HttpException(null, filterContext.Exception).GetHttpCode();
                //if (statusCode == 500)
                //{
                //    filterContext.Result = new RedirectResult("/Error/Page500");
                //    filterContext.HttpContext.Response.StatusCode = 500;
                //}
                //else if (statusCode == 404)
                //{
                //    filterContext.Result = new RedirectResult("/Error/Page404");
                //    filterContext.HttpContext.Response.StatusCode = 404;
                //}
            }
            //filterContext.HttpContext.Response.Clear();
            //filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;

        }

    }
}