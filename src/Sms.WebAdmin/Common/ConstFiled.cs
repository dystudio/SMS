using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sms.WebAdmin.Common
{
    public class ConstFiled
    {
        /// <summary>
        /// 登录用户的session名称
        /// </summary>
        public static readonly string Admin_Session_Name = "LoginedAdmin";

        /// <summary>
        /// 每页显示数据条数
        /// </summary>
        public static readonly int PageSize = 20;

        /// <summary>
        /// 菜单的缓存key
        /// </summary>
        public static readonly string GlobalMenu = "MenuCache";
    }
}