using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Sms.WebAdmin.Models
{
    public class ApiResponseMessage
    {
        public ApiResponseMessage() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="status"></param>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        public ApiResponseMessage(ResultStatus status, string msg = "", object data = null)
        {
            this.Status = status;
            this.Message = msg;
            this.Data = data;
        }

        /// <summary>
        /// 返回状态码
        /// </summary>
        public ResultStatus Status { get; set; }

        /// <summary>
        /// 返回消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 返回的数据
        /// </summary>
        public object Data { get; set; }
    }

    public enum ResultStatus
    {
        /// <summary>
        /// 服务异常
        /// </summary>
        [Description("服务异常")]
        ServiceError = -2,

        /// <summary>
        /// 请求失败
        /// </summary>
        [Description("请求失败")]
        Failed = -1,

        /// <summary>
        /// 请求成功
        /// </summary>
        [Description("请求成功")]
        Success = 0,

        /// <summary>
        /// 登录失败
        /// </summary>
        [Description("登录失败")]
        UnAuthorize = 1,

        /// <summary>
        /// 参数异常
        /// </summary>
        [Description("参数异常")]
        ParamError = 2,

        /// <summary>
        /// 数据异常
        /// </summary>
        [Description("数据异常")]
        DataException = 3,

        /// <summary>
        /// 验证失败
        /// </summary>
        [Description("验证失败")]
        ValidateError = 4
    }
}