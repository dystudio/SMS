using Sms.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sms.WebAdmin.Filter
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class PermissionFilterAttribute : Attribute
    {
        /// <summary>
        /// 待验证的操作集合
        /// </summary>
        public List<EnumHepler.ActionPermission> Code { get; set; }

        /// <summary>
        /// 权限检查 过滤器
        /// </summary>
        /// <param name="action">权限判断操作码</param>
        /// <param name="isAddorEdit">注:此功能只限同一个action处理新增修改的权限判断,并且修改是以id作为主键参数传递!</param>
        public PermissionFilterAttribute(bool isAddorEdit = false, params EnumHepler.ActionPermission[] action)
        {
            if (isAddorEdit)
            {
                this.Code = new List<EnumHepler.ActionPermission>();
                int ID = 0;
                if (int.TryParse(System.Web.HttpContext.Current.Request.UrlReferrer.Segments.LastOrDefault(), out ID) && ID > 0)
                {
                    this.Code.Add(EnumHepler.ActionPermission.Edit);
                }
                else
                {
                    this.Code.Add(EnumHepler.ActionPermission.New);
                }
            }
            else
            {
                this.Code = action.ToList();
            }
        }
    }
}