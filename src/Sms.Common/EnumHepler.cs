using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sms.Common
{
    /// <summary>
    /// 枚举辅助类
    /// </summary>
    public class EnumHepler
    {
        public enum Demo
        {
            [Description("first")]
            First = 1,

            [Description("two")]
            Two = 2
        }

        /// <summary>
        /// 促销类型
        /// </summary>
        public enum PromotionType
        {
            [Description("会员卡注册")]
            Register = 1,

            [Description("会员卡充值")]
            Charge = 2,

            [Description("会员卡消费")]
            Consume = 3
        }

        /// <summary>
        /// 促销状态
        /// </summary>
        public enum PromotionStatus
        {
            [Description("已删除")]
            Deleted = -1,

            [Description("禁用")]
            Locked = 0,

            [Description("有效")]
            Available = 1
        }

        /// <summary>
        /// 会员卡状态
        /// </summary>
        public enum MemberCardStatus
        {
            [Description("禁用")]
            Locked = 0,

            [Description("有效")]
            Available = 1
        }

        /// <summary>
        /// 日志类型
        /// </summary>
        public enum LogType
        {
            /// <summary>
            /// 登陆账号
            /// </summary>
            [Description("账号登录")]
            Login = 1,

            /// <summary>
            /// 注销账号 
            /// </summary>
            [Description("注销账号")]
            LoginOut = 2,

            /// <summary>
            /// 数据操作 
            /// </summary>
            [Description("数据操作")]
            Oprate = 3
        }

        /// <summary>
        /// 系统操作权限
        /// </summary>
        public enum ActionPermission
        {
            /// <summary>
            /// 查看
            /// </summary>
            [Description("查看")]
            View = 1,

            /// <summary>
            /// 添加
            /// </summary>
            [Description("添加")]
            New = 2,

            /// <summary>
            /// 修改
            /// </summary>
            [Description("修改")]
            Edit = 3,

            /// <summary>
            /// 删除
            /// </summary>
            [Description("删除")]
            Delete = 4,

            /// <summary>
            /// 充值
            /// </summary>
            [Description("充值")]
            Charge = 5,

            /// <summary>
            /// 消费
            /// </summary>
            [Description("消费")]
            Consume = 6,

            /// <summary>
            /// 导出
            /// </summary>
            [Description("导出")]
            Export = 7,

            /// <summary>
            /// 查询会员卡
            /// </summary>
            [Description("查询会员卡")]
            SearchCard = 8,

            /// <summary>
            /// 发送短信
            /// </summary>
            [Description("发送短信")]
            Message = 9,

            /// <summary>
            /// 修改密码
            /// </summary>
            [Description("修改密码")]
            Password = 10,

            /// <summary>
            /// 查看权限
            /// </summary>
            [Description("查看权限")]
            ViewRight = 11,

            /// <summary>
            /// 变更权限
            /// </summary>
            [Description("变更权限")]
            ChangeRight = 12,

            /// <summary>
            /// 变更状态
            /// </summary>
            [Description("变更状态")]
            ChangeStatus = 13,

            /// <summary>
            /// 上传图片
            /// </summary>
            [Description("上传图片")]
            UpImage = 14,

            /// <summary>
            /// 上传图片
            /// </summary>
            [Description("上传文件")]
            UpFile = 15
        }

        /// <summary>
        /// 角色状态
        /// </summary>
        public enum RoleStatus
        {
            /// <summary>
            /// 已删除
            /// </summary>
            [Description("已删除")]
            Deleted = -1,

            /// <summary>
            /// 禁用
            /// </summary>
            [Description("禁用")]
            Lock = 0,

            /// <summary>
            /// 有效
            /// </summary>
            [Description("有效")]
            Available = 1
        }


        /// <summary>
        /// 用户状态
        /// </summary>
        public enum UserStatus
        {
            /// <summary>
            /// 锁定
            /// </summary>
            [Description("锁定")]
            Lock = 0,

            /// <summary>
            /// 正常
            /// </summary>
            [Description("正常")]
            Available = 1
        }

        /// <summary>
        /// 根据枚举值获取描述
        /// </summary>
        /// <param name="value">枚举值</param>
        /// <returns></returns>
        /// 用法：int value = 1;GetEnumDescription((MyEnum)value)
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }

        /// <summary>
        /// 根据枚举值获取描述
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <param name="typeValue">枚举值</param>
        /// <returns></returns>
        public static string GetEnumDescription(Type enumType, int typeValue)
        {
            Type typeDescription = typeof(DescriptionAttribute);
            System.Reflection.FieldInfo[] fields = enumType.GetFields();
            string strText = string.Empty;

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType.IsEnum)
                {
                    if (typeValue == ((int)enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null)))
                    {
                        object[] arr = field.GetCustomAttributes(typeDescription, true);
                        if (arr.Length > 0)
                        {
                            DescriptionAttribute aa = (DescriptionAttribute)arr[0];
                            strText = aa.Description;
                        }
                        else
                        {
                            strText = field.Name;
                        }
                        break;
                    }
                }
            }
            return strText;
        }

        /// <summary>
        /// 获取枚举类型的数据字典
        /// </summary>
        /// <param name="enu">枚举类型</param>
        /// <returns></returns>
        public static Dictionary<string, int> GetEnumData(Type enu)
        {
            Dictionary<string, int> list = new Dictionary<string, int>();
            var array = Enum.GetValues(enu);
            foreach (var item in array)
            {
                int value = (int)item;
                list.Add(GetEnumDescription(enu, value), value);
            }
            return list;
        }
    }
}
