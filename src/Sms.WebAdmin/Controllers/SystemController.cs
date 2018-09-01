using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sms.WebAdmin.Filter;
using Sms.Common;
using Sms.Entity;
using System.Threading.Tasks;
using Sms.Entity.ViewModel;
using Webdiyer.WebControls.Mvc;
using Sms.WebAdmin.Common;
using System.Xml;

namespace Sms.WebAdmin.Controllers
{
    public class SystemController : BaseController
    {

        #region 系统模块管理部分

        /// <summary>
        /// 模块树形列表页
        /// </summary>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult Module()
        {
            return View(_repositoryFactory.ISystemModule.Where(m => m.Id > 0).ToList());
        }

        /// <summary>
        /// 模块编辑页
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult ModuleEdit(int? id)
        {
            var ddlParent = new SelectList(_repositoryFactory.ISystemModule.Where(m => m.IsDisplay && m.ParentId == 0, m => m.Sort, true), "Id", "Name").ToList();
            ddlParent.Insert(0, new SelectListItem() { Text = "无", Value = "0" });
            ViewBag.ddlParent = ddlParent;
            var model = _repositoryFactory.ISystemModule.Single(m => m.Id == id);
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
        /// 模块编辑逻辑处理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilterAttribute(true, EnumHepler.ActionPermission.New)]
        public async Task<ActionResult> ModuleEdit(SystemModule model)
        {
            if (ModelState.IsValid)
            {
                List<int> rightList = new List<int>();
                if (!string.IsNullOrEmpty(Request.Params["rightlist"]))
                {
                    rightList = Request.Params["rightlist"].Split(',').Select(m => int.Parse(m)).ToList();
                }
                if (model.Id != 0)
                {
                    _repositoryFactory.ISystemModule.Modify(model, "Name", "ParentId", "Sort", "Url", "IsDisplay");
                    //修改权限
                    var existRight = _repositoryFactory.ISystemModuleRight.Where(m => m.ModuleId == model.Id).ToList();
                    //从新权限中移除已存在的，删除数据库中多余的
                    foreach (var exist in existRight)
                    {
                        if (rightList.Contains(exist.RightId))
                        {
                            rightList.Remove(exist.RightId);
                        }
                        else
                        {
                            _repositoryFactory.ISystemModuleRight.Delete(exist);
                        }
                    }
                    //剩下的新权限就要新增了
                    if (rightList.Count() > 0)
                    {
                        foreach (int r in rightList)
                        {
                            _repositoryFactory.ISystemModuleRight.Add(new SystemModuleRight() { ModuleId = model.Id, RightId = r });
                        }
                    }
                    if (await _repositoryFactory.SaveChanges() > 0)
                    {
                        return ShowResultMessage(new TipMessage() { Status = true, MsgText = "编辑成功！", Url = Url.Action("Module") });
                    }
                    else
                    {
                        return ShowResultMessage(new TipMessage() { Status = false, MsgText = "编辑失败！" });
                    }
                }
                else
                {
                    model.CreateTime = DateTime.Now;
                    model.CreateUser = CurrentLoginUser.UserName;
                    _repositoryFactory.ISystemModule.Add(model);
                    //增加权限
                    if (rightList.Count() > 0)
                    {
                        foreach (int r in rightList)
                        {
                            _repositoryFactory.ISystemModuleRight.Add(new SystemModuleRight() { ModuleId = model.Id, RightId = r });
                        }
                    }
                    if (await _repositoryFactory.SaveChanges() > 0)
                    {
                        return ShowResultMessage(new TipMessage() { Status = true, MsgText = "添加成功！", Url = Url.Action("Module") });
                    }
                    else
                    {
                        return ShowResultMessage(new TipMessage() { Status = false, MsgText = "添加失败！" });
                    }
                }
            }
            return ShowResultMessage(new TipMessage() { Status = false, MsgText = "未知操作结果！", Url = Url.Action("Module") });
        }

        /// <summary>
        /// 异步加载模块拥有的权限
        /// </summary>
        /// <returns>json数据</returns>
        [HttpPost]
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public JsonResult LoadModuleRight(int module)
        {
            var list = new List<SelectItem>();
            //模块拥有的权限
            List<int> existRight = new List<int>();
            if (module != 0)
            {
                existRight = _repositoryFactory.ISystemModuleRight.Where(m => m.ModuleId == module).Select(m => m.RightId).ToList();
            }
            //所有权限
            var allRight = EnumHepler.GetEnumData(typeof(EnumHepler.ActionPermission));
            foreach (var right in allRight)
            {
                list.Add(new SelectItem() { Text = right.Key, Value = right.Value, Selected = existRight.Contains(right.Value) });
            }
            return Json(new TipMessage() { Status = true, Data = list }, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// 异步删除模块
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.Delete)]
        public async Task<ActionResult> ModuleDelete(int module)
        {
            //1.先检查是否有子模块
            var childCount = _repositoryFactory.ISystemModule.Count(m => m.ParentId == module);
            if (childCount > 0)
            {
                return Json(new TipMessage() { Status = false, MsgText = "该模块下有子模块，无法删除！" });
            }
            //2.删除关联的信息
            _repositoryFactory.ISystemModuleRight.DeleteBy(m => m.ModuleId == module);
            _repositoryFactory.ISystemRoleRight.DeleteBy(m => m.ModuleId == module);
            //3.删除模块记录
            _repositoryFactory.ISystemModule.DeleteBy(m => m.Id == module);
            if (await _repositoryFactory.SaveChanges() > 0)
            {
                return Json(new TipMessage() { Status = true, MsgText = "删除成功" });
            }
            else
            {
                return Json(new TipMessage() { Status = false, MsgText = "删除失败" });
            }
        }
        #endregion


        /// <summary>
        /// 系统日志页，分页，搜索
        /// </summary>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult Log(int? type, string keyword = "")
        {
            ViewBag.ddlType = new SelectList(EnumHepler.GetEnumData(typeof(EnumHepler.LogType)), "Value", "Key");
            var list = _repositoryFactory.ISysLog.Where(c => c.Id > 0);
            //搜索关键字过滤
            if (type != null && type != -1)
            {
                list = list.Where(c => c.Type == type);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                list = list.Where(c => c.Remark.Contains(keyword));
            }
            list = list.OrderByDescending(c => c.CreateTime);
            var pagerList = list.ToPagedList(PageIndex, ConstFiled.PageSize);
            return View(pagerList);
        }

        /// <summary>
        /// 请求源设置
        /// </summary>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult RequestControl()
        {
            IpConfig config = new IpConfig() { };
            lock (this)
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(HttpContext.Server.MapPath("/App_Data/iptable.xml"));
                config.Status = xml.SelectSingleNode("root/IpControl").InnerText.ToLower();
                config.AddressList = new List<string>();
                XmlNodeList router = xml.SelectSingleNode("root/Router").ChildNodes;
                foreach (var item in router)
                {
                    config.AddressList.Add(((XmlNode)item).InnerText);
                }
            }
            return View(config);
        }

        [HttpPost, PermissionFilterAttribute(false, EnumHepler.ActionPermission.Edit)]
        public ActionResult RequestControl(FormCollection form)
        {
            IpConfig config = new IpConfig() { };
            lock (this)
            {
                try
                {
                    string status = form["ipstatus"];
                    string[] router = form["address"].ToString().Split(',');
                    XmlDocument xml = new XmlDocument();
                    string path = HttpContext.Server.MapPath("/App_Data/iptable.xml");
                    xml.Load(path);
                    xml.SelectSingleNode("root/IpControl").InnerText = status ;

                    var routerNode = xml.SelectSingleNode("root/Router");
                    routerNode.RemoveAll();
                    foreach (var item in router)
                    {
                        XmlElement xe = xml.CreateElement("Address");
                        xe.InnerText = item;
                        routerNode.AppendChild(xe);
                    }
                    xml.Save(path);
                    WriteLog($"修改了IP白名单的信息");
                    _repositoryFactory.SaveChanges();
                    return Json(new TipMessage() { Status = true, MsgText = "保存成功！" }, JsonRequestBehavior.DenyGet);
                }
                catch (Exception)
                {

                }
            }
            return Json(new TipMessage() { Status = false, MsgText = "保存失败！" }, JsonRequestBehavior.DenyGet);
        }
    }
}
