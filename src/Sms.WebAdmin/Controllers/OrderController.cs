using Sms.Common;
using Sms.WebAdmin.Common;
using Sms.WebAdmin.Filter;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace Sms.WebAdmin.Controllers
{
    public class OrderController : BaseController
    {
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult Index(int? level, string keyword = "")
        {
            var list = _repositoryFactory.IOrders.Where(c =>true);
            //搜索关键字过滤
            //if (level.HasValue)
            //{
            //    list = list.Where(c => c.BrandId == level.Value);
            //}
            if (!string.IsNullOrEmpty(keyword))
            {
                list = list.Where(c => c.OrderCode.Equals(keyword) || c.OpenId.Equals(keyword));
            }
            var pagerList = list.OrderByDescending(c => c.CreateTime).ToPagedList(PageIndex, ConstFiled.PageSize);
            if (Request.IsAjaxRequest())
                return PartialView("_PartialOrderList", pagerList);
            return View(pagerList);
        }
    }
}