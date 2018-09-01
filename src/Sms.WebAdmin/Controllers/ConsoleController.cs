using Sms.Common;
using Sms.Entity.ViewModel;
using Sms.WebAdmin.Common;
using Sms.WebAdmin.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace Sms.WebAdmin.Controllers
{
    public class ConsoleController : BaseController
    {
        /// <summary>
        /// 控制台全局框架
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            //根据登录用户的权限获取菜单节点，放到缓存中，下次直接从缓存读取
            List<MenuNode> menu = new List<MenuNode>();
            var cache = CacheHelper.GetCache(ConstFiled.GlobalMenu + CurrentLoginUser.Id);
            if (cache != null)
            {
                menu = cache as List<MenuNode>;
            }
            else
            {
                //1.从数据库查询
                //模块列表
                var moduleList = _repositoryFactory.ISystemModule.Where(m => m.IsDisplay && m.ParentId != 0, m => m.Sort, true).ToList();
                if (!CurrentLoginUser.IsSuperUser)
                {
                    if (!string.IsNullOrEmpty(CurrentLoginUser.RoleString))
                    {
                        IEnumerable<int> role = CurrentLoginUser.RoleString.Split(',').Select(m => Convert.ToInt32(m));
                        //找到拥有查看权限的模块id
                        var roleRight = _repositoryFactory.ISystemRoleRight.Where(m => role.Contains(m.RoleId) && m.RightId == (int)EnumHepler.ActionPermission.View).Select(m => m.ModuleId).Distinct();
                        //从所有模块中移除没有查看权限的
                        moduleList.RemoveAll(m => !roleRight.Contains(m.Id));
                    }
                    else
                    {
                        //一个角色都么有肯定也就没有任何菜单了
                        moduleList.Clear();
                    }
                }
                var top = _repositoryFactory.ISystemModule.Where(m => m.IsDisplay && m.ParentId == 0, m => m.Sort, true).Select(m => new { m.Id, m.Name, m.Url }).ToList();
                foreach (var module in top)
                {
                    var child = moduleList.Where(m => m.ParentId == module.Id).OrderBy(m => m.Sort);
                    if (child.Count() > 0)
                    {
                        MenuNode node = new MenuNode();
                        node.Title = module.Name;
                        node.Id = module.Id;
                        foreach (var c in child)
                        {
                            node.ChildNode.Add(new MenuNode() { Id = c.Id, Title = c.Name, Url = c.Url });
                        }
                        menu.Add(node);
                    }
                }
                //2.放入缓存
                CacheHelper.SetCache(ConstFiled.GlobalMenu + CurrentLoginUser.Id, menu, TimeSpan.FromMinutes(10));
            }
            return View(menu);
        }

        /// <summary>
        /// 控制台首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Home()
        {
            return View();
        }

        /// <summary>
        /// 获取当前登录用户的信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetLoginUserDetail()
        {
            var current = CurrentLoginUser;
            if (current.IsSuperUser)
            {
                string path = HttpContext.Server.MapPath("/App_Data/adminconfig.xml");
                XmlDocument config = new XmlDocument();
                config.Load(path);
                return Json(new TipMessage()
                {
                    Status = true,
                    MsgText = "获取信息成功",
                    Data = new
                    {
                        UserName = config.SelectSingleNode("root/SuperUser").InnerText,
                        TrueName = "超级管理员",
                        LinkPhone = "",
                        LastLoginTime = config.SelectSingleNode("root/LastLoginTime").InnerText,
                        CreateTime = "系统自动创建",
                        RoleList = "所有"
                    }
                }, JsonRequestBehavior.DenyGet);
            }
            var user = _repositoryFactory.ISystemUser.Single(m => m.Id == current.Id);
            if (user != null)
            {
                StringBuilder roleString = new StringBuilder();
                int[] roleArry = user.RoleList.Split(',').Select(m => int.Parse(m)).ToArray();
                var roles = _repositoryFactory.ISystemRole.Where(m => roleArry.Contains(m.Id)).Select(m => m.Name).ToList();
                foreach (var item in roles)
                {
                    roleString.Append(item + "、");
                }
                return Json(new TipMessage()
                {
                    Status = true,
                    MsgText = "获取信息成功",
                    Data = new
                    {
                        user.UserName,
                        user.TrueName,
                        user.LinkPhone,
                        LastLoginTime = user.LastLoginTime != null ? user.LastLoginTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                        CreateTime = user.CreateTime != null ? user.CreateTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                        RoleList = roleString.Length > 0 ? roleString.ToString().TrimEnd('、') : ""
                    }
                }, JsonRequestBehavior.DenyGet);
            }
            return Json(new TipMessage() { Status = false, MsgText = "获取信息失败" }, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// 清除缓存和session
        /// </summary>
        /// <returns></returns>
        public ActionResult ClearCache()
        {
            var cache = HttpContext.Cache.GetEnumerator();
            while (cache.MoveNext())
            {
                HttpContext.Cache.Remove(cache.Key.ToString());
            }
            Session.Clear();
            return Json(new TipMessage() { Status = true, MsgText = "清除成功！" }, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// 当前用户修改密码
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> ChangePassword(ChangePassword model)
        {
            if (ModelState.IsValid)
            {
                if (!model.ConfirmNewPwd.Equals(model.NewPwd))
                {
                    return ShowResultMessage(new TipMessage() { Status = false, MsgText = "确认新密码错误！" });
                }
                var current = CurrentLoginUser;
                if (current.IsSuperUser)
                {
                    string path = HttpContext.Server.MapPath("/App_Data/adminconfig.xml");
                    XmlDocument config = new XmlDocument();
                    config.Load(path);
                    var old = config.SelectSingleNode("root/SuperUserPassword").InnerText;
                    if (old.Equals(SecurityHelper.MD5(model.OldPwd)))
                    {

                        config.SelectSingleNode("root/SuperUserPassword").InnerText = SecurityHelper.MD5(model.NewPwd);
                        config.Save(path);
                        return ShowResultMessage(new TipMessage() { Status = true, MsgText = "修改成功！", Url = Url.Action("LoginOut", "Login") });
                    }
                    else
                    {
                        return ShowResultMessage(new TipMessage() { Status = false, MsgText = "旧密码输入错误！" });
                    }
                }
                else
                {
                    var user = await _repositoryFactory.ISystemUser.SingleAsync(m => m.Id == CurrentLoginUser.Id);
                    if (!user.Password.Equals(SecurityHelper.MD5(model.OldPwd)))
                    {
                        return ShowResultMessage(new TipMessage() { Status = false, MsgText = "旧密码输入错误！" });
                    }
                    user.Password = SecurityHelper.MD5(model.NewPwd);
                    _repositoryFactory.ISystemUser.Modify(user, "Password");
                    if (await _repositoryFactory.SaveChanges() > 0)
                    {
                        return ShowResultMessage(new TipMessage() { Status = true, MsgText = "修改成功！", Url = Url.Action("LoginOut", "Login") });
                    }
                    else
                    {
                        return ShowResultMessage(new TipMessage() { Status = false, MsgText = "修改失败！" });
                    }
                }
            }
            return View();
        }
    }
}
