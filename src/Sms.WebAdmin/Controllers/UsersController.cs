using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using Sms.WebAdmin.Filter;
using Sms.Common;
using Sms.WebAdmin.Common;
using Sms.Entity;
using System.Threading.Tasks;
using Sms.Entity.ViewModel;

namespace Sms.WebAdmin.Controllers
{
    public class UsersController : BaseController
    {
        #region 用户基础信息维护
        /// <summary>
        /// 用户列表页，分页，搜索
        /// </summary>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult Index( string keyword = "")
        {
            var list = _repositoryFactory.ISystemUser.Where(c => c.Id > 0);
            //搜索关键字过滤
            if (!string.IsNullOrEmpty(keyword))
            {
                list = list.Where(c => c.UserName.Contains(keyword) || c.TrueName.Contains(keyword));
            }
            list = list.OrderByDescending(c => c.CreateTime);
            var pagerList = list.ToPagedList(PageIndex, ConstFiled.PageSize);
            return View(pagerList);
        }

        /// <summary>
        /// 用户信息编辑页
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult Edit(int? id)
        {
            ViewBag.ddlRole = new SelectList(_repositoryFactory.ISystemRole.Where(m => m.Status == (int)EnumHepler.RoleStatus.Available, m => m.Sort, true), "Id", "Name");
            var model = _repositoryFactory.ISystemUser.Single(m => m.Id == id);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return View();
            }
        }

        /// <summary>
        /// 用户信息编辑处理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilterAttribute(true, EnumHepler.ActionPermission.New)]
        public async Task<ActionResult> Edit(SystemUser model)
        {
            if (ModelState.IsValid)
            {
                model.RoleList = Request.Params["rolelist"];
                if (model.Id != 0)
                {
                    _repositoryFactory.ISystemUser.Modify(model, "UserName", "TrueName", "LinkPhone", "RoleList");
                    if (await _repositoryFactory.SaveChanges() > 0)
                    {
                        return ShowResultMessage(new TipMessage() { Status = true, MsgText = "编辑成功！", Url = Url.Action("Index") });
                    }
                    else
                    {
                        return ShowResultMessage(new TipMessage() { Status = false, MsgText = "编辑失败！", Url = Url.Action("Edit", new { id = model.Id }) });
                    }
                }
                else
                {
                    model.Password = SecurityHelper.MD5(model.Password);
                    model.CreateTime = DateTime.Now;
                    model.CreateUser = CurrentLoginUser.UserName;
                    _repositoryFactory.ISystemUser.Add(model);
                    if (await _repositoryFactory.SaveChanges() > 0)
                    {
                        return ShowResultMessage(new TipMessage() { Status = true, MsgText = "添加成功！", Url = Url.Action("Index") });
                    }
                    else
                    {
                        return ShowResultMessage(new TipMessage() { Status = false, MsgText = "添加失败！", Url = Url.Action("Index") });
                    }
                }
            }
            return ShowResultMessage(new TipMessage() { Status = false, MsgText = "未知操作结果！", Url = Url.Action("Index") });
        }

        /// <summary>
        /// 异步检查用户名是否唯一
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckUserName(string userName, string oldUserName)
        {
            if (!string.IsNullOrEmpty(oldUserName) && userName.Equals(oldUserName))
            {
                return Content("true");//编辑的时候没有改用户名不用验证
            }
            if(userName.Equals("admin"))
            {
                return Content("false");
            }
            var member = _repositoryFactory.ISystemUser.Single(m => m.UserName == userName);
            if (member != null)
            {
                return Content("false");
            }
            else
            {
                return Content("true");
            }
        }

        /// <summary>
        /// 修改用户状态
        /// </summary>
        /// <param name="id">用户id</param>
        /// <param name="status">更新的状态编号</param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.ChangeStatus)]
        public async Task<ActionResult> UpdateStatus(int id, int status)
        {
            var entity = _repositoryFactory.ISystemUser.Single(m => m.Id == id);
            if (entity != null)
            {
                entity.Status = status;
                _repositoryFactory.ISystemUser.Modify(entity, "Status");
                if (await _repositoryFactory.SaveChanges() > 0)
                {
                    return Json(new TipMessage() { Status = true, MsgText = "操作成功", Url = Url.Action("Index") }, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new TipMessage() { Status = false, MsgText = "操作失败" }, JsonRequestBehavior.DenyGet);
        }


        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="id">用户id</param>
        /// <param name="type">修改类型</param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.Password)]
        public async Task<ActionResult> UpdatePassword(int id, int type)
        {
            var entity = _repositoryFactory.ISystemUser.Single(m => m.Id == id);
            if (entity != null)
            {
                string newPwd = SecurityHelper.MD5(System.Configuration.ConfigurationManager.AppSettings["InitPassword"]);
                if (type == 1)
                {
                    entity.Password = newPwd;
                    _repositoryFactory.ISystemUser.Modify(entity, "Password");
                }
                else if (type == 2)
                {
                    entity.OpratePwd = newPwd;
                    _repositoryFactory.ISystemUser.Modify(entity, "OpratePwd");
                }
                if (await _repositoryFactory.SaveChanges() > 0)
                {
                    return Json(new TipMessage() { Status = true, MsgText = "操作成功", Url = Url.Action("Index") }, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new TipMessage() { Status = false, MsgText = "操作失败" }, JsonRequestBehavior.DenyGet);
        }
        #endregion


    }
}
