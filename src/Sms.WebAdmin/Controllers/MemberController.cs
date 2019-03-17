using NPOI.SS.UserModel;
using Sms.Common;
using Sms.Entity;
using Sms.Entity.ViewModel;
using Sms.WebAdmin.Common;
using Sms.WebAdmin.Filter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace Sms.WebAdmin.Controllers
{
    public class MemberController : BaseController
    {
        #region 会员卡基本信息
        /// <summary>
        /// 会员列表页，分页，搜索
        /// </summary>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult Index(int? level, string keyword = "")
        {
            var list = _repositoryFactory.IWeChatMember.Where(c => true);
            //搜索关键字过滤
            if (level.HasValue)
            {
                list = list.Where(c => c.Rank == level.Value);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                list = list.Where(c => c.Mobile.Contains(keyword) || c.NickName.Contains(keyword));
            }
            list = list.OrderByDescending(c => c.CreateTime);
            var pagerList = list.ToPagedList(PageIndex, ConstFiled.PageSize);
            if (Request.IsAjaxRequest())
                return PartialView("_PartialMemberList", pagerList);
            return View(pagerList);
        }


        /// <summary>
        /// 会员卡信息编辑页
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult Edit(string no = "")
        {
            if (no == "")
            {
                return View();
            }
            var model = _repositoryFactory.IMemberCard.Single(m => m.CardNo == no);
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
        public async Task<ActionResult> Edit(MemberCard model)
        {
            if (ModelState.IsValid)
            {
                string editMode = Request.Params["mode"];
                if (editMode == "edit")
                {
                    _repositoryFactory.IMemberCard.Modify(model, "Mobile", "Name", "Sex", "Status");
                    WriteLog($"修改了会员卡【{model.CardNo}】的信息");
                    if (await _repositoryFactory.SaveChanges() > 0)
                    {
                        return ShowResultMessage(new TipMessage() { Status = true, MsgText = "编辑成功！", Url = Url.Action("Index") });
                    }
                    else
                    {
                        return ShowResultMessage(new TipMessage() { Status = false, MsgText = "编辑失败！", Url = Url.Action("Edit", new { no = model.CardNo }) });
                    }
                }
                else
                {
                    model.Banlance = 0;
                    model.TotalMoney = 0;
                    model.TotalPresent = 0;
                    model.TotalDiscount = 0;
                    model.Password = SecurityHelper.MD5(model.Password);
                    model.CreateTime = DateTime.Now;
                    model.CreateUser = CurrentLoginUser.UserName;
                    //查找是否有优惠
                    var promote = GetAvailablePromotion(EnumHepler.PromotionType.Register, decimal.MaxValue);
                    if (promote != null)
                    {
                        _repositoryFactory.ICardHistory.Add(new CardHistory()
                        {
                            CardNo = model.CardNo,
                            PracticalValue = promote.Money.Value,
                            PromotionValue = promote.Money.Value,
                            Value = 0,
                            CreateTime = DateTime.Now,
                            CreateUser = model.CreateUser,
                            Type = (int)EnumHepler.BillType.Register_Gifts,
                            Remark = $"参与促销活动[{promote.Title}]赠送{promote.Money}元"
                        });
                        model.Banlance += promote.Money.Value;
                        model.TotalPresent += promote.Money;
                    }
                    _repositoryFactory.IMemberCard.Add(model);
                    WriteLog($"创建了会员卡【{model.CardNo}】");
                    if (await _repositoryFactory.SaveChanges() > 0)
                    {
                        return ShowResultMessage(new TipMessage() { Status = true, MsgText = "添加成功！", Url = Url.Action("Edit") });
                    }
                    else
                    {
                        return ShowResultMessage(new TipMessage() { Status = false, MsgText = "添加失败！", Url = Url.Action("Edit") });
                    }
                }
            }
            return ShowResultMessage(new TipMessage() { Status = false, MsgText = "未知操作结果！", Url = Url.Action("Index") });
        }

        /// <summary>
        /// 异步检查会员卡号是否唯一
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckCardNo(string CardNo)
        {
            var member = _repositoryFactory.IMemberCard.Single(m => m.CardNo.Equals(CardNo));
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
        /// 异步检查手机号是否唯一
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckMobile(string mobile, string oldmobile)
        {
            if (!string.IsNullOrEmpty(mobile) && mobile.Equals(oldmobile))
            {
                return Content("true");//编辑的时候没有改手机号不用验证
            }
            var member = _repositoryFactory.IMemberCard.Single(m => m.Mobile == mobile);
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
        /// 删除会员卡
        /// </summary>
        /// <param name="id">用户id</param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.Delete)]
        public async Task<ActionResult> MemberDelete(string id)
        {
            _repositoryFactory.IMemberCard.DeleteBy(m => m.CardNo.Equals(id));
            WriteLog($"删除了会员卡【{id}】");
            if (await _repositoryFactory.SaveChanges() > 0)
            {
                return Json(new TipMessage() { Status = true, MsgText = "删除成功" }, JsonRequestBehavior.DenyGet);
            }
            return Json(new TipMessage() { Status = false, MsgText = "删除失败" }, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// 修改会员卡状态
        /// </summary>
        /// <param name="id">用户id</param>
        /// <param name="status">更新的状态编号</param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.ChangeStatus)]
        public async Task<ActionResult> UpdateStatus(string id, int status)
        {
            var entity = _repositoryFactory.IWeChatMember.Single(m => m.OpenId == id);
            if (entity != null)
            {
                entity.Status = status;
                _repositoryFactory.IWeChatMember.Modify(entity, "Status");
                WriteLog($"变更了会员【{id}】的状态");
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
        /// <returns></returns>
        [HttpPost]
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.Password)]
        public async Task<ActionResult> UpdatePassword(string id)
        {
            var entity = _repositoryFactory.IMemberCard.Single(m => m.CardNo == id);
            if (entity != null)
            {
                string newPwd = SecurityHelper.MD5(System.Configuration.ConfigurationManager.AppSettings["InitPassword"]);
                entity.Password = newPwd;
                _repositoryFactory.IMemberCard.Modify(entity, "Password");
                WriteLog($"重置了会员卡【{id}】的消费密码");
                if (await _repositoryFactory.SaveChanges() > 0)
                {
                    return Json(new TipMessage() { Status = true, MsgText = "操作成功", Url = Url.Action("Index") }, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new TipMessage() { Status = false, MsgText = "操作失败" }, JsonRequestBehavior.DenyGet);
        }


        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.Export)]
        public JsonResult ExportMember(string keyword = "")
        {
            var list = _repositoryFactory.IMemberCard.Where(c => true);
            //搜索关键字过滤
            if (!string.IsNullOrEmpty(keyword))
            {
                list = list.Where(c => c.CardNo.Contains(keyword) || c.Mobile.Contains(keyword));
            }
            var data = list.OrderByDescending(m => m.CreateTime).ToList();
            if (data.Count > 0)
            {
                //NPOI导出数据
                NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
                ISheet sheet = book.CreateSheet("sheet1");
                ICellStyle style1 = book.CreateCellStyle();
                style1.Alignment = HorizontalAlignment.Center;
                style1.VerticalAlignment = VerticalAlignment.Center;
                ICellStyle style2 = book.CreateCellStyle();
                style2.Alignment = HorizontalAlignment.Left;
                style2.VerticalAlignment = VerticalAlignment.Center;
                IRow headerrow = sheet.CreateRow(0);
                ICell cell_1 = headerrow.CreateCell(0);
                cell_1.CellStyle = style1;
                cell_1.SetCellValue("会员卡号");
                ICell cell_2 = headerrow.CreateCell(1);
                cell_2.CellStyle = style1;
                cell_2.SetCellValue("手机号");
                ICell cell_3 = headerrow.CreateCell(2);
                cell_3.CellStyle = style1;
                cell_3.SetCellValue("姓名");
                ICell cell_4 = headerrow.CreateCell(3);
                cell_4.CellStyle = style1;
                cell_4.SetCellValue("性别");
                ICell cell_5 = headerrow.CreateCell(4);
                cell_5.CellStyle = style1;
                cell_5.SetCellValue("创建时间");
                ICell cell_6 = headerrow.CreateCell(5);
                cell_6.CellStyle = style1;
                cell_6.SetCellValue("创建人");
                ICell cell_7 = headerrow.CreateCell(6);
                cell_7.CellStyle = style1;
                cell_7.SetCellValue("状态");
                ICell cell_8 = headerrow.CreateCell(7);
                cell_8.CellStyle = style1;
                cell_8.SetCellValue("可用余额");
                ICell cell_9 = headerrow.CreateCell(8);
                cell_9.CellStyle = style1;
                cell_9.SetCellValue("累计充值");
                foreach (var item in data)
                {
                    IRow headerrow_1 = sheet.CreateRow(data.IndexOf(item) + 1);
                    ICell cell_1_1 = headerrow_1.CreateCell(0);
                    cell_1_1.CellStyle = style1;
                    cell_1_1.SetCellValue(item.CardNo);
                    ICell cell_1_2 = headerrow_1.CreateCell(1);
                    cell_1_2.CellStyle = style1;
                    cell_1_2.SetCellValue(item.Mobile);
                    ICell cell_1_3 = headerrow_1.CreateCell(2);
                    cell_1_3.CellStyle = style1;
                    cell_1_3.SetCellValue(item.Name);
                    ICell cell_1_4 = headerrow_1.CreateCell(3);
                    cell_1_4.CellStyle = style1;
                    cell_1_4.SetCellValue(item.Sex);
                    ICell cell_1_5 = headerrow_1.CreateCell(4);
                    cell_1_5.CellStyle = style1;
                    cell_1_5.SetCellValue(string.Format("{0:yyyy-MM-dd HH:mm:ss}", item.CreateTime));
                    ICell cell_1_6 = headerrow_1.CreateCell(5);
                    cell_1_6.CellStyle = style1;
                    cell_1_6.SetCellValue(item.CreateUser);
                    ICell cell_1_7 = headerrow_1.CreateCell(6);
                    cell_1_7.CellStyle = style1;
                    cell_1_7.SetCellValue(EnumHepler.GetEnumDescription(((EnumHepler.MemberCardStatus)item.Status)));
                    ICell cell_1_8 = headerrow_1.CreateCell(7);
                    cell_1_8.CellStyle = style1;
                    cell_1_8.SetCellValue(string.Format("{0:N2}", item.Banlance));
                    ICell cell_1_9 = headerrow_1.CreateCell(8);
                    cell_1_9.CellStyle = style1;
                    cell_1_9.SetCellValue(string.Format("{0:N2}", item.TotalMoney));
                }
                sheet.SetColumnWidth(1, 30 * 150);
                sheet.SetColumnWidth(3, 30 * 250);
                string fileName = "会员卡列表_" + DateTime.Now.ToString("yyyy-MM-dd") + ".xls";
                string filePath = HttpContext.Server.MapPath("/Upload/Export/" + fileName);
                using (FileStream fs = System.IO.File.OpenWrite(filePath))
                {
                    book.Write(fs);//向打开的这个xls文件中写入并保存。  
                }
                return Json(new TipMessage() { Status = true, MsgText = "导出成功！", Url = Url.Action("DownLoadFile", "FileHandler", new { path = filePath, content = "application/ms-excel" }) }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(new TipMessage() { Status = false, MsgText = "暂无会员卡记录！" }, JsonRequestBehavior.DenyGet);
            }
        }
        #endregion


        #region 会员卡充值

        /// <summary>
        /// 会员卡充值页面
        /// </summary>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult Charge()
        {
            return View();
        }

        [HttpPost]
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.Charge)]
        public async Task<ActionResult> Charge(CardHistory model)
        {
            string pwd = SecurityHelper.MD5(Request.Params["opratepwd"]);
            var user = CurrentLoginUser;
            if (pwd.Equals(user.OpratePwd))
            {
                var card = await _repositoryFactory.IMemberCard.SingleAsync(m => m.Status == (int)EnumHepler.MemberCardStatus.Available && m.CardNo.Equals(model.CardNo));
                if (card != null)
                {
                    //写充值记录
                    model.Type = (int)EnumHepler.BillType.Charge;
                    model.CreateTime = DateTime.Now;
                    model.CreateUser = user.UserName;
                    model.Remark = $"管理员[{user.UserName}]为会员卡[{model.CardNo}]充值{model.Value}元";
                    //查找是否有优惠
                    var promote = GetAvailablePromotion(EnumHepler.PromotionType.Charge, model.Value);
                    if (promote != null)
                    {
                        model.PromotionValue = promote.Money.Value;
                        model.Remark += $"<br /> P：[{promote.Title}] 赠送{promote.Money}元";
                    }
                    model.PracticalValue = model.Value + model.PromotionValue;
                    _repositoryFactory.ICardHistory.Add(model);
                    //更新会员卡信息
                    card.Banlance += model.PracticalValue;//余额
                    card.TotalMoney += model.Value;//累计充值
                    card.TotalPresent += model.PromotionValue;//累计赠送
                    //写系统日志
                    WriteLog(model.Remark);
                    //保存数据
                    if (await _repositoryFactory.SaveChanges() > 0)
                    {
                        return ShowResultMessage(new TipMessage() { Status = true, MsgText = "充值成功！" }, "ClearCharge();");
                    }
                    else
                    {
                        return ShowResultMessage(new TipMessage() { Status = false, MsgText = "充值失败！", Url = Url.Action("Charge") });
                    }
                }
                else
                {
                    return ShowResultMessage(new TipMessage() { Status = false, MsgText = "查询会员卡失败！" });
                }
            }
            else
            {
                return ShowResultMessage(new TipMessage() { Status = false, MsgText = "操作密码错误！" });
            }
        }

        /// <summary>
        /// 充值记录页面
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult ChargeHistory(DateTime? start, DateTime? end, string keyword = "")
        {
            var list = _repositoryFactory.ICardHistory.Where(c => c.Type == (int)EnumHepler.BillType.Register_Gifts || c.Type == (int)EnumHepler.BillType.Charge);
            //搜索关键字过滤
            if (start != null)
            {
                list = list.Where(c => c.CreateTime >= start);
            }
            if (end != null)
            {
                list = list.Where(c => c.CreateTime <= end);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                list = list.Where(c => c.CardNo.Contains(keyword));
            }
            list = list.OrderByDescending(c => c.Id);
            var pagerList = list.ToPagedList(PageIndex, ConstFiled.PageSize);
            return View(pagerList);
        }

        /// <summary>
        /// 根据关键字查询会员卡信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.SearchCard)]
        public ActionResult SearchMemberCard(string key)
        {
            var entity = _repositoryFactory.IMemberCard.Single(m => m.Status == (int)EnumHepler.MemberCardStatus.Available && (m.CardNo.Equals(key) || m.Mobile.Equals(key)));
            if (entity != null)
            {
                return Json(new TipMessage() { Status = true, MsgText = "查询成功", Data = new { entity.CardNo, Banlance = entity.Banlance.ToString(), entity.Name, entity.Mobile, CreateTime = entity.CreateTime.Value.ToShortDateString() } }, JsonRequestBehavior.DenyGet);
            }
            return Json(new TipMessage() { Status = false, MsgText = "会员卡未注册" }, JsonRequestBehavior.DenyGet);
        }

        public async Task<ActionResult> HistoryDelete(int id)
        {
            var entity = await _repositoryFactory.ICardHistory.SingleAsync(m => m.Id == id);
            if (entity != null)
            {
                _repositoryFactory.ICardHistory.Delete(entity);
                WriteLog($"删除了会员卡{entity.CardNo}记录，内容：{entity.Remark}");
                if (await _repositoryFactory.SaveChanges() > 0)
                {
                    return Json(new TipMessage() { Status = true, MsgText = "删除成功" }, JsonRequestBehavior.DenyGet);
                }
            }
            return Json(new TipMessage() { Status = false, MsgText = "删除失败" }, JsonRequestBehavior.DenyGet);
        }
        #endregion

        #region 会员卡消费

        /// <summary>
        /// 会员卡消费页面
        /// </summary>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult Order()
        {
            return View();
        }

        [HttpPost]
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.Consume)]
        public async Task<ActionResult> Order(CardHistory model)
        {
            //string pwd = SecurityHelper.MD5(Request.Params["opratepwd"]);
            var user = CurrentLoginUser;
            //if (pwd.Equals(user.OpratePwd))
            {
                var card = await _repositoryFactory.IMemberCard.SingleAsync(m => m.Status == (int)EnumHepler.MemberCardStatus.Available && m.CardNo.Equals(model.CardNo));
                if (card != null)
                {
                    //查找是否有优惠
                    var promote = GetAvailablePromotion(EnumHepler.PromotionType.Consume, model.Value);
                    if (promote != null)
                    {
                        model.PromotionValue = promote.Money.Value;
                        model.Remark += $" <br /> P：[{promote.Title}] 优惠{promote.Money}元";
                    }
                    model.PracticalValue = model.Value - model.PromotionValue;
                    if (card.Banlance < model.PracticalValue)
                    {
                        return ShowResultMessage(new TipMessage() { Status = false, MsgText = $"会员卡余额不足！折扣：{model.PromotionValue}，待支付：{model.PracticalValue}" });
                    }
                    //写消费记录
                    model.Type = (int)EnumHepler.BillType.Pay;
                    model.CreateTime = DateTime.Now;
                    model.CreateUser = user.UserName;
                    _repositoryFactory.ICardHistory.Add(model);
                    //更新会员卡信息
                    card.Banlance -= model.PracticalValue;//余额
                    card.TotalDiscount += model.PromotionValue;//累计折扣
                    //写系统日志
                    WriteLog($"管理员[{user.UserName}]为会员卡[{model.CardNo}]消费{model.Value}元，备注：{model.Remark}");
                    //保存数据
                    if (await _repositoryFactory.SaveChanges() > 0)
                    {
                        return ShowResultMessage(new TipMessage() { Status = true, MsgText = $"消费成功！{(model.PromotionValue > 0 ? $"本次省下{model.PromotionValue}元" : "")}" }, "ClearConsume();");
                    }
                    else
                    {
                        return ShowResultMessage(new TipMessage() { Status = false, MsgText = "消费失败！", Url = Url.Action("Order") });
                    }
                }
                else
                {
                    return ShowResultMessage(new TipMessage() { Status = false, MsgText = "查询会员卡失败！" });
                }
            }
            //else
            //{
            //    return ShowResultMessage(new TipMessage() { Status = false, MsgText = "操作密码错误！" });
            //}
        }


        /// <summary>
        /// 消费记录页面
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult OrderHistory(DateTime? start, DateTime? end, string keyword = "")
        {
            var list = _repositoryFactory.ICardHistory.Where(c => c.Type == (int)EnumHepler.BillType.Pay);
            //搜索关键字过滤
            if (start != null)
            {
                list = list.Where(c => c.CreateTime >= start);
            }
            if (end != null)
            {
                list = list.Where(c => c.CreateTime <= end);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                list = list.Where(c => c.CardNo.Contains(keyword));
            }
            list = list.OrderByDescending(c => c.Id);
            var pagerList = list.ToPagedList(PageIndex, ConstFiled.PageSize);
            return View(pagerList);
        }
        #endregion
    }
}