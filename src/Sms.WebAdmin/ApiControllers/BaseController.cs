using Sms.WebAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Sms.Common;
using Sms.Entity;

namespace Sms.WebAdmin.ApiControllers
{
    public class BaseController : ApiController
    {
        [Ninject.Inject]
        protected Sms.IRepository.IRepositoryFactory _repositoryFactory { get; set; }

        [AuthorizationAttribute]
        protected WeChatMember CurrentUser
        {
            get
            {
                string token = Request.Headers.FirstOrDefault(x => x.Key == "token").Value.FirstOrDefault();
                token = SecurityHelper.DecryptDES(token, Common.ConstFiled.OpenIdEncryptKey);
                var user = CacheHelper.GetCache(token) as WeChatMember;
                if (user == null)
                {
                    user = _repositoryFactory.IWeChatMember.Single(x => x.OpenId == token);
                }
                return user;
            }
        }

        /// <summary>
        /// 接口统一的返回消息
        /// </summary>
        /// <param name="result">返回内容</param>
        /// <returns></returns>
        protected HttpResponseMessage ApiResponse(ApiResponseMessage result)
        {
            HttpResponseMessage response = new HttpResponseMessage
            {
                Content = new ObjectContent<ApiResponseMessage>(result, Configuration.Formatters.JsonFormatter),
            };
            return response;
        }

        protected HttpResponseMessage ApiResponse(ResultStatus status, string message, object data = null)
        {
            HttpResponseMessage response = new HttpResponseMessage
            {
                Content = new ObjectContent<ApiResponseMessage>(new ApiResponseMessage(status, message, data), Configuration.Formatters.JsonFormatter),
            };
            return response;
        }

        protected HttpResponseMessage ApiResponse(object data = null)
        {
            HttpResponseMessage response = new HttpResponseMessage
            {
                Content = new ObjectContent<object>(data, Configuration.Formatters.JsonFormatter),
            };
            return response;
        }
    }

    /// <summary>
    /// 权限验证过滤器
    /// author：hoho
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, Inherited = true)]
    public class AuthorizationAttribute : AuthorizeAttribute
    {
        [Ninject.Inject]
        public Sms.IRepository.IRepositoryFactory _repositoryFactory { get; set; }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            // 匿名访问验证
            var anonymousAction = actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>();
            if (!anonymousAction.Any())
            {
                ApiResponseMessage result = null;
                // token存在验证
                var token = actionContext.ControllerContext.Request.Headers.FirstOrDefault(x => x.Key == "token").Value.FirstOrDefault();
                if (string.IsNullOrEmpty(token))
                {
                    result = new ApiResponseMessage(ResultStatus.UnAuthorize, "未登录");
                }
                else
                {
                    token = SecurityHelper.DecryptDES(token, Common.ConstFiled.OpenIdEncryptKey);
                    //token合法验证
                    var sign = CacheHelper.GetCache(token);
                    if (sign == null)
                    {
                        var user = _repositoryFactory.IWeChatMember.Single(x => x.OpenId == token);
                        if (user == null)
                        {
                            result = new ApiResponseMessage(ResultStatus.UnAuthorize, "未注册用户");
                        }
                        else
                        {
                            CacheHelper.SetCache(token, user, TimeSpan.FromHours(5));
                        }
                    }
                }
                if (result != null)
                {
                    var response = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.Unauthorized };
                    response.Content = new ObjectContent<ApiResponseMessage>(result,
                        actionContext.ActionDescriptor.Configuration.Formatters.JsonFormatter);
                    actionContext.Response = response;
                    return;
                }
            }
        }
    }
}