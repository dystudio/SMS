using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sms.WebAdmin.Controllers
{
    public class ErrorController : BaseController
    {
        /// <summary>
        /// 没有查看权限
        /// </summary>
        /// <returns></returns>
        public ActionResult NoViewPermission()
        {
            return View();
        }

        /// <summary>
        /// 没有操作权限
        /// </summary>
        /// <returns></returns>
        public ActionResult NoActionPermission()
        {
            return View();
        }

        /// <summary>
        /// 404错误提示页
        /// </summary>
        /// <returns></returns>
        public ActionResult Page_404()
        {
            return View();
        }

        /// <summary>
        /// 500服务错误提示页
        /// </summary>
        /// <returns></returns>
        public ActionResult Page_500()
        {
            return View();
        }
        
    }
}
