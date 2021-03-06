﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sms.Entity.ViewModel;
using Sms.Common;
using Sms.WebAdmin.Common;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using System.Web.Security;

namespace Sms.WebAdmin.Controllers
{
    public class LoginController : Controller
    {
        [Ninject.Inject]
        protected Sms.IRepository.IRepositoryFactory _repositoryFactory { get; set; }

        /// <summary>
        /// 系统登录页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            if (!CheckIpControl())
            {
                return Redirect("http://www.baidu.com");
            }
            return View();
        }

        /// <summary>
        /// 登录业务处理
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Index(string username, string password)
        {
            if (!CheckIpControl())
            {
                return Redirect("http://www.baidu.com");
            }
            string notice = string.Empty;
            AdminLoginModel model = new AdminLoginModel();
            bool loginSuccess = false;
            string path = HttpContext.Server.MapPath("/App_Data/adminconfig.xml");
            XmlDocument config = new XmlDocument();
            config.Load(path);
            password = SecurityHelper.MD5(password);
            string SuperUser = config.SelectSingleNode("root/SuperUser").InnerText;
            if (username.Equals(SuperUser))
            {
                string SuperUserPassword = config.SelectSingleNode("root/SuperUserPassword").InnerText;
                if (SuperUserPassword.Equals(password))
                {
                    model.Id = -1;
                    model.UserName = SuperUser;
                    model.Password = SuperUserPassword;
                    model.OpratePwd = SuperUserPassword;
                    model.IsSuperUser = true;
                    config.SelectSingleNode("root/LastLoginTime").InnerText = config.SelectSingleNode("root/CurrentLoginTime").InnerText;
                    config.SelectSingleNode("root/CurrentLoginTime").InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    config.Save(path);
                    loginSuccess = true;
                }
                else
                {
                    return new JavaScriptResult() { Script = "show_message('用户名或密码错误！')" };
                }
            }
            else
            {
                var user = _repositoryFactory.ISystemUser.Single(m => m.UserName == username && m.Password == password && m.Status == (int)EnumHepler.UserStatus.Available);
                if (user != null)
                {
                    model.Id = user.Id;
                    model.UserName = username;
                    model.Password = password;
                    model.OpratePwd = SecurityHelper.MD5(user.OpratePwd);
                    model.IsSuperUser = false;
                    model.RoleString = user.RoleList;
                    user.LastLoginTime = user.CurrentLoginTime;
                    user.CurrentLoginTime = DateTime.Now;
                    _repositoryFactory.ISystemUser.Modify(user, "LastLoginTime", "CurrentLoginTime");
                    loginSuccess = true;
                }
                else
                {
                    return new JavaScriptResult() { Script = "show_message('用户名或密码错误！')" };
                }
            }
            if (loginSuccess)
            {
                //写登录日志
                string loginLog = $"登录账号：{model.UserName}，登录IP：{CommonTools.GetIpAddress()}";
                _repositoryFactory.ISysLog.Add(new Entity.SysLog() { CreateTime = DateTime.Now, Type = (int)EnumHepler.LogType.Login, Remark = loginLog });
                await _repositoryFactory.SaveChanges();

                //序列化admin对象
                string accountJson = JsonConvert.SerializeObject(model);
                //创建用户票据
                var ticket = new FormsAuthenticationTicket(1, model.UserName, DateTime.Now, DateTime.Now.AddDays(1), false, accountJson);
                //加密
                string encryptAccount = FormsAuthentication.Encrypt(ticket);
                //创建cookie
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptAccount)
                {
                    HttpOnly = true,
                    Secure = FormsAuthentication.RequireSSL,
                    Domain = FormsAuthentication.CookieDomain,
                    Path = FormsAuthentication.FormsCookiePath
                };
                cookie.Expires = DateTime.Now.AddDays(1);
                //写入Cookie
                Response.Cookies.Remove(cookie.Name);
                Response.Cookies.Add(cookie);
                return new JavaScriptResult() { Script = "login_status=true;location.href='" + Url.Action("Index", "Console") + "'" };
            }
            else
            {
                return new JavaScriptResult() { Script = "show_message('登录失败！')" };
            }
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> LoginOut()
        {
            HttpCookie cookie = HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
            {
                return RedirectToAction("Index");
            }
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
            AdminLoginModel model = JsonConvert.DeserializeObject<AdminLoginModel>(ticket.UserData);
            if (model != null)
            {
                //写注销日志                
                string loginLog = $"退出账号：{model.UserName}，登录IP：{CommonTools.GetIpAddress()}";
                _repositoryFactory.ISysLog.Add(new Entity.SysLog() { CreateTime = DateTime.Now, Type = (int)EnumHepler.LogType.LoginOut, Remark = loginLog });
                await _repositoryFactory.SaveChanges();

                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Set(cookie);
            }
            return RedirectToAction("Index");
        }

        private bool CheckIpControl()
        {
            lock (this)
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(HttpContext.Server.MapPath("/App_Data/iptable.xml"));
                string status = xml.SelectSingleNode("root/IpControl").InnerText.ToLower();
                if (status != "true")
                {
                    return true;
                }
                List<string> addressList = new List<string>();
                XmlNodeList router = xml.SelectSingleNode("root/Router").ChildNodes;
                foreach (var item in router)
                {
                    addressList.Add(((XmlNode)item).InnerText);
                }
                if (addressList.Contains(CommonTools.GetIpAddress()))
                {
                    return true;
                }
                return false;
            }
        }
    }
}
