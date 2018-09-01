using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.Entity.ViewModel
{
    public class AdminLoginModel
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 登录用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 操作密码
        /// </summary>
        public string OpratePwd { get; set; }

        /// <summary>
        /// 是否超级管理员
        /// </summary>
        public bool IsSuperUser { get; set; }

        /// <summary>
        /// 拥有的角色
        /// </summary>
        public string RoleString { get; set; }
    }
}
