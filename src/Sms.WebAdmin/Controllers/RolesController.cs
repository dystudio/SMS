using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sms.Common;
using Sms.WebAdmin.Filter;
using Sms.WebAdmin.Common;
using Webdiyer.WebControls.Mvc;
using Sms.Entity;
using Sms.Entity.ViewModel;
using System.Threading.Tasks;

namespace Sms.WebAdmin.Controllers
{
    public class RolesController : BaseController
    {
        #region 角色的基础信息维护

        /// <summary>
        /// 角色列表页，分页，搜索
        /// </summary>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult Index(string keyword = "")
        {
            var list = _repositoryFactory.ISystemRole.Where(c => c.Status != (int)EnumHepler.RoleStatus.Deleted);
            //搜索关键字过滤
            if (!string.IsNullOrEmpty(keyword))
            {
                list = list.Where(c => c.Name.Contains(keyword) || c.Remark.Contains(keyword));
            }
            list = list.OrderByDescending(m => m.Sort);
            var pagerList = list.ToPagedList(PageIndex, ConstFiled.PageSize);
            return View(pagerList);
        }

        /// <summary>
        /// 角色信息编辑页
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult Edit(int? id)
        {
            var model = _repositoryFactory.ISystemRole.Single(m => m.Id == id);
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
        /// 角色信息编辑处理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilterAttribute(true, EnumHepler.ActionPermission.New)]
        public async Task<ActionResult> Edit(SystemRole model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id != 0)
                {
                    _repositoryFactory.ISystemRole.Modify(model, "Name", "Sort", "Remark");
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
                    model.Status = (int)EnumHepler.RoleStatus.Available;
                    model.CreateTime = DateTime.Now;
                    model.CreateUser = CurrentLoginUser.UserName;
                    _repositoryFactory.ISystemRole.Add(model);
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
        /// 修改角色状态
        /// </summary>
        /// <param name="id">角色id</param>
        /// <param name="status">更新的状态编号</param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.ChangeStatus)]
        public async Task<ActionResult> UpdateStatus(int id, int status)
        {
            var entity = _repositoryFactory.ISystemRole.Single(m => m.Id == id);
            if (entity != null)
            {
                entity.Status = status;
                _repositoryFactory.ISystemRole.Modify(entity, "Status");
                if (await _repositoryFactory.SaveChanges() > 0)
                {
                    return Json(new TipMessage() { Status  = true, MsgText = "操作成功" }, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new TipMessage() { Status  = false, MsgText = "操作失败" }, JsonRequestBehavior.DenyGet);
        }
        #endregion


        #region 角色权限操作

        /// <summary>
        /// 权限编辑页面
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.ViewRight)]
        public ActionResult RightConfig(int id)
        {
            ViewBag.RoleId = id;
            //模块列表
            var moduleList = _repositoryFactory.ISystemModule.WhereJoin(m => m.IsDisplay, m => m.Sort, true, "SystemModuleRight").ToList();
            //该角色已保存的权限
            var existRight = _repositoryFactory.ISystemRoleRight.Where(m => m.RoleId == id).ToList();
            //页面展示的数据
            var viewList = new List<RoleModuleRight>();
            foreach (var module in moduleList)
            {
                var viewModel = new RoleModuleRight();
                viewModel.Name = module.Name;
                viewModel.Parent = module.ParentId;
                viewModel.Value = module.Id;
                viewModel.Sort = module.Sort;
                foreach (var mr in module.SystemModuleRight)
                {
                    viewModel.RightList.Add(new ModuleRight()
                    {
                        Value = mr.RightId,
                        Text = EnumHepler.GetEnumDescription(typeof(EnumHepler.ActionPermission), mr.RightId),
                        Checked = existRight.Count(m => m.RightId == mr.RightId && m.ModuleId == module.Id) > 0
                    });
                }
                viewList.Add(viewModel);
            }
            return View(viewList);
        }

        /// <summary>
        /// 保存页面选择的权限
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.ChangeRight)]
        public async Task<ActionResult> SaveModuleRight(string rightStr, int role)
        {
            try
            {
                if (string.IsNullOrEmpty(rightStr))
                {
                    //为空的时候是清空所有权限
                    _repositoryFactory.ISystemRoleRight.DeleteBy(m => m.RoleId == role);
                }
                else
                {
                    //不为空的时候要新增和删除
                    List<string> rightArry = rightStr.TrimEnd(',').Split(',').ToList();
                    var existRight = _repositoryFactory.ISystemRoleRight.Where(m => m.RoleId == role).ToList();
                    //从新权限中移除已存在的，删除数据库中多余的
                    string viewStr = string.Empty;
                    foreach (var exist in existRight)
                    {
                        viewStr = exist.RightId + "|" + exist.ModuleId;
                        if (rightArry.Contains(viewStr))
                        {
                            rightArry.Remove(viewStr);
                        }
                        else
                        {
                            _repositoryFactory.ISystemRoleRight.Delete(exist);
                        }
                    }
                    //剩下的新权限就要新增了
                    if (rightArry.Count() > 0)
                    {
                        foreach (string r in rightArry)
                        {
                            var values = r.Split('|').Select(m => Convert.ToInt32(m)).ToArray();
                            _repositoryFactory.ISystemRoleRight.Add(new SystemRoleRight() { ModuleId = values[1], RightId = values[0], RoleId = role });
                        }
                    }
                }
                await _repositoryFactory.SaveChanges();
                return Json(new TipMessage(){ Status = true, MsgText = "保存成功！", Url = Url.Action("RightConfig", new { id = role }) });
            }
            catch (Exception ex)
            {
                return Json(new TipMessage() { Status = false, MsgText = "保存失败！" });
            }
        }
        #endregion
    }
}
