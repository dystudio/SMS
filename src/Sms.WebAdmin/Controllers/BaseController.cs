using Sms.Common;
using Sms.Entity.ViewModel;
using Sms.WebAdmin.Common;
using Sms.WebAdmin.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sms.Entity;
using System.Web.Security;
using Newtonsoft.Json;

namespace Sms.WebAdmin.Controllers
{
    public class BaseController : Controller
    {
        [Ninject.Inject]
        protected Sms.IRepository.IRepositoryFactory _repositoryFactory { get; set; }

        protected readonly string tokenName = FormsAuthentication.FormsCookieName;

        /// <summary>
        /// 当前登录的用户
        /// </summary>
        protected AdminLoginModel CurrentLoginUser
        {
            get
            {
                HttpCookie cookie = HttpContext.Request.Cookies[tokenName];
                if (cookie == null || string.IsNullOrEmpty(cookie.Value))
                {
                    return null;
                }
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                AdminLoginModel admin = JsonConvert.DeserializeObject<AdminLoginModel>(ticket.UserData);
                return admin;
            }
        }

        /// <summary>
        /// 当前页码
        /// </summary>
        protected int PageIndex
        {
            get
            {
                int page = 0;
                int.TryParse(System.Web.HttpContext.Current.Request.Params["page"], out page);
                return page == 0 ? 1 : page;
            }
        }

        /// <summary>
        /// 返回脚本提示信息
        /// </summary>
        /// <param name="result">操作结果</param>
        /// <param name="msg">输出的信息</param>
        /// <param name="url">跳转路径</param>
        /// <returns></returns>
        protected JavaScriptResult ShowResultMessage(TipMessage tipMsg, string callBack = "")
        {
            return new JavaScriptResult() { Script = "show_message(" + (tipMsg.Status ? "true" : "false") + ",'" + tipMsg.MsgText + "','" + tipMsg.Url + "');" + callBack + "" };
        }

        /// <summary>
        /// 写系统日志
        /// </summary>
        /// <param name="type"></param>
        /// <param name="remark"></param>
        protected void WriteLog(string content)
        {
            _repositoryFactory.ISysLog.Add(new Entity.SysLog() { CreateTime = DateTime.Now, Type = (int)EnumHepler.LogType.Oprate, Remark = $"[{CurrentLoginUser.UserName}] — " + content });
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //增加登录token的有效时间
            HttpCookie token = Request.Cookies[tokenName];
            if (token != null)
            {
                token.Expires = DateTime.Now.AddDays(1);
                Response.Cookies.Set(token);
            }
            base.OnActionExecuted(filterContext);
        }

        /// <summary>
        /// 登录和操作权限验证
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            var accept = filterContext.HttpContext.Request.AcceptTypes;
            bool isJsonRequest = accept.Contains("application/json");
            //登录验证
            if (CurrentLoginUser == null)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    if (isJsonRequest)
                    {
                        filterContext.Result = new JsonResult()
                        {
                            Data = new TipMessage() { Status = false, MsgText = "您尚未登录或登录超时！正在跳转...", Url = Url.Action("Index", "Login") },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }
                    else
                    {
                        filterContext.Result = new JavaScriptResult { Script = "show_message(false,'您尚未登录或登录超时！正在跳转...','" + Url.Action("Index", "Login") + "');" };
                    }
                }
                else
                {
                    RedirectResult _RedirectResult = new RedirectResult("~/Login");
                    filterContext.Result = _RedirectResult;
                }
            }
            else
            {
                //操作权限验证
                bool isAuthorized = false;
                string failAction = string.Empty;
                if (CurrentLoginUser.IsSuperUser)
                {
                    //超级管理员拥有所有权限，所以不用检查了
                    isAuthorized = true;
                }
                else
                {
                    var Attributes = filterContext.ActionDescriptor.GetCustomAttributes(typeof(PermissionFilterAttribute), false);
                    if (Attributes.Length > 0)
                    {
                        //标记了操作权限验证的过滤器
                        var PermissionCheck = Attributes[0] as PermissionFilterAttribute;
                        //判断是否拥有对应的操作权限
                        int module = 0;
                        //从cookie中取出当前操作的模块
                        int.TryParse(CookieHelper.GetCookie("thenode"), out module);
                        if (module != 0)
                        {
                            //开始判断
                            if (!string.IsNullOrEmpty(CurrentLoginUser.RoleString))
                            {
                                IEnumerable<int> role = CurrentLoginUser.RoleString.Split(',').Select(m => Convert.ToInt32(m));
                                bool validate = true;
                                int checkCount = 0;
                                foreach (var code in PermissionCheck.Code)
                                {
                                    //找到数据库里的权限记录数量
                                    checkCount = _repositoryFactory.ISystemRoleRight.Count(m => role.Contains(m.RoleId) && m.RightId == (int)code && m.ModuleId == module);
                                    if (checkCount == 0)
                                    {
                                        validate = false;
                                        failAction = EnumHepler.GetEnumDescription(code);
                                        break;//有一个验证失败剩下的就不用验证了，直接是没权限
                                    }
                                }
                                isAuthorized = validate;
                            }
                        }
                    }
                    else
                    {
                        //没有添加权限验证属性的就认为是可以操作的
                        isAuthorized = true;
                    }
                }
                if (!isAuthorized)
                {
                    if (filterContext.HttpContext.Request.IsAjaxRequest())
                    {
                        if (isJsonRequest)
                        {
                            filterContext.Result = new JsonResult()
                            {
                                Data = new TipMessage() { Status = false, MsgText = "您没有" + failAction + "权限！" },
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            };
                        }
                        else
                        {
                            filterContext.Result = new JavaScriptResult
                            {
                                Script = "show_message(false,'您没有" + failAction + "权限！',null);"
                            };
                        }
                    }
                    else
                    {
                        filterContext.Result = new RedirectResult("~/Error/NoViewPermission");
                    }
                }
            }

        }

        /// <summary>
        /// 根据促销类型查找可用的优惠
        /// </summary>
        /// <param name="type"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        protected Promotion GetAvailablePromotion(EnumHepler.PromotionType type, decimal min)
        {
            Promotion model = null;
            DateTime start = DateTime.Now;
            DateTime end = start.AddDays(-1);
            var promote = _repositoryFactory.IPromotion.Where(m => m.Type == (int)type && m.Status == (int)EnumHepler.PromotionStatus.Available && m.StartDate < start && m.EndDate > end).OrderByDescending(m => m.MinValue).ThenByDescending(m => m.CreateTime).ToList();
            foreach (var item in promote)
            {
                if (min >= item.MinValue)
                {
                    model = item;
                    break;
                }
            }
            return model;
        }
    }
}