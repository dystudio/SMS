//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Sms.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class SystemUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string OpratePwd { get; set; }
        public string TrueName { get; set; }
        public string LinkPhone { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public int Status { get; set; }
        public string CreateUser { get; set; }
        public string RoleList { get; set; }
        public Nullable<System.DateTime> LastLoginTime { get; set; }
        public Nullable<System.DateTime> CurrentLoginTime { get; set; }
    }
}