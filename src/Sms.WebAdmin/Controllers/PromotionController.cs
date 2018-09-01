using Sms.Common;
using Sms.Entity;
using Sms.Entity.ViewModel;
using Sms.WebAdmin.Common;
using Sms.WebAdmin.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace Sms.WebAdmin.Controllers
{
    public class PromotionController : BaseController
    {
        /// <summary>
        /// 促销活动列表页
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult Index(string keyword = "")
        {
            var list = _repositoryFactory.IPromotion.Where(c => c.Status != (int)EnumHepler.PromotionStatus.Deleted);
            //搜索关键字过滤
            if (!string.IsNullOrEmpty(keyword))
            {
                list = list.Where(c => c.Title.Contains(keyword));
            }
            list = list.OrderByDescending(c => c.CreateTime);
            var pagerList = list.ToPagedList(PageIndex, ConstFiled.PageSize);
            return View(pagerList);
        }

        /// <summary>
        /// 促销活动信息编辑页
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult Edit(int? id )
        {
            if (id == 0)
            {
                return View();
            }
            var model = _repositoryFactory.IPromotion.Single(m => m.Id == id);
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
        /// 促销活动信息编辑处理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilterAttribute(true, EnumHepler.ActionPermission.New)]
        public async Task<ActionResult> Edit(Promotion model)
        {
            if (ModelState.IsValid)
            {
                string editMode = Request.Params["mode"];
                if (editMode == "edit")
                {
                    _repositoryFactory.IPromotion.Modify(model, "Title", "Type", "StartDate", "EndDate", "MinValue", "Money", "Status");
                    WriteLog($"修改了促销活动【{model.Title}】的信息");
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
                    model.CreateTime = DateTime.Now;
                    model.CreateUser = CurrentLoginUser.UserName;
                    _repositoryFactory.IPromotion.Add(model);
                    WriteLog($"创建了促销活动【{model.Title}】");
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
        /// 修改状态
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="status">更新的状态编号</param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.ChangeStatus)]
        public async Task<ActionResult> UpdateStatus(int id, int status)
        {
            var entity = _repositoryFactory.IPromotion.Single(m => m.Id == id);
            if (entity != null)
            {
                entity.Status = status;
                _repositoryFactory.IPromotion.Modify(entity, "Status");
                WriteLog($"变更了促销活动【{entity.Title}】的状态");
                if (await _repositoryFactory.SaveChanges() > 0)
                {
                    return Json(new TipMessage() { Status = true, MsgText = "操作成功", Url = Url.Action("Index") }, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new TipMessage() { Status = false, MsgText = "操作失败" }, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.Delete)]
        public async Task<ActionResult> PromotionDelete(int id)
        {
            var entity = _repositoryFactory.IPromotion.Single(m => m.Id == id);
            if (entity != null)
            {
                 entity.Status = (int)EnumHepler.PromotionStatus.Deleted;
                _repositoryFactory.IPromotion.Modify(entity, "Status");
                WriteLog($"删除了促销活动【{entity.Title}】");
                if (await _repositoryFactory.SaveChanges() > 0)
                {
                    return Json(new TipMessage() { Status = true, MsgText = "删除成功"}, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new TipMessage() { Status = false, MsgText = "删除失败" }, JsonRequestBehavior.DenyGet);
        }
    }
}