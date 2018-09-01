using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Sms.Common;
using Sms.WebAdmin.Filter;
using Sms.Entity.ViewModel;

namespace Sms.WebAdmin.Controllers
{
    public class ReportController : BaseController
    {
        [PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public async Task<ActionResult> Index()
        {
            DateTime nextDay = DateTime.Today.AddDays(1);
            //今日新增会员卡
            ViewData["TodayNewCard"] = await _repositoryFactory.IMemberCard.CountAsync(m => m.CreateTime >= DateTime.Today && m.CreateTime < nextDay);
            //会员卡总数
            ViewData["TotalCard"] = await _repositoryFactory.IMemberCard.CountAsync(m => true);
            //今日销售总额
            var TodayOrderMoney = _repositoryFactory.ICardHistory.Where(m => m.CreateTime >= DateTime.Today && m.CreateTime < nextDay && m.Type == 2).Select(m => m.Value);
            ViewData["TodayOrderMoney"] = 0.00;
            if (TodayOrderMoney.Any())
            {
                ViewData["TodayOrderMoney"] = TodayOrderMoney.Sum();
            }
            //历史销售总额
            ViewData["TotalOrderMoney"] = _repositoryFactory.ICardHistory.Where(m => m.Type == 2).Select(m => m.Value).Sum();
            //今日充值总额
            var TodayChargeMoney = _repositoryFactory.ICardHistory.Where(m => m.CreateTime >= DateTime.Today && m.CreateTime < nextDay && m.Type == 1).Select(m => m.Value);
            ViewData["TodayChargeMoney"] = 0.00;
            if (TodayChargeMoney.Any())
            {
                ViewData["TodayChargeMoney"] = TodayChargeMoney.Sum();
            }
            //本月充值总额
            DateTime thisMonth = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
            ViewData["MonthChargeMoney"] = _repositoryFactory.ICardHistory.Where(m => m.CreateTime >= thisMonth && m.CreateTime < nextDay && m.Type == 1).Select(m => m.Value).Sum();
            //历史充值总额
            ViewData["TotalChargeMoney"] = _repositoryFactory.ICardHistory.Where(m => m.Type == 1).Select(m => m.Value).Sum();
            //会员卡总余额
            ViewData["TotalBanlance"] = _repositoryFactory.IMemberCard.Where(m => true).Select(m => m.Banlance).Sum();
            return View();
        }


        #region 图标异步方法

        /// <summary>
        /// 累计消费排行榜
        /// </summary>
        /// <returns></returns>
        [HttpPost, PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public JsonResult ConsumeRank()
        {
            var data = _repositoryFactory.IMemberCard.Where(m => m.TotalMoney > 0).Select(m => new { m.Name, m.CardNo, ConsumeMoney = m.TotalMoney + m.TotalPresent - m.Banlance }).OrderByDescending(m => m.ConsumeMoney).Take(30).ToList();
            List<string> title = new List<string>();
            List<decimal> value = new List<decimal>();
            foreach (var item in data)
            {
                title.Add(item.CardNo + "/" + item.Name);
                value.Add((decimal)item.ConsumeMoney);
            }
            return Json(new TipMessage() { Status = true, MsgText = "获取数据成功！", Data = new { legend = title, value = value } }, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// 一周销售数据
        /// </summary>
        /// <returns></returns>
        [HttpPost, PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public JsonResult WeekOrder()
        {
            DateTime weekStart = DateTime.Today.AddDays(-6);
            var data = _repositoryFactory.ICardHistory.Where(m => m.Type == 2 && m.CreateTime >= weekStart).Select(m => new { m.CreateTime, m.Value }).ToList();
            List<string> title = new List<string>();
            List<decimal> value = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                string format = DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd");
                title.Add(format);
                value.Add(data.Where(m => m.CreateTime.Value.ToString("yyyy-MM-dd").Equals(format)).Sum(m => m.Value));
            }
            return Json(new TipMessage() { Status = true, MsgText = "获取数据成功！", Data = new { legend = title, value = value } }, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// 一周充值数据
        /// </summary>
        /// <returns></returns>
        [HttpPost, PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public JsonResult WeekCharge()
        {
            DateTime weekStart = DateTime.Today.AddDays(-6);
            var data = _repositoryFactory.ICardHistory.Where(m => m.Type == 1 && m.CreateTime >= weekStart).Select(m => new { m.CreateTime, m.Value }).ToList();
            List<string> title = new List<string>();
            List<decimal> value = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                string format = DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd");
                title.Add(format);
                value.Add(data.Where(m => m.CreateTime.Value.ToString("yyyy-MM-dd").Equals(format)).Sum(m => m.Value));
            }
            return Json(new TipMessage() { Status = true, MsgText = "获取数据成功！", Data = new { legend = title, value = value } }, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// 会员卡性别比例数据
        /// </summary>
        /// <returns></returns>
        [HttpPost, PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public async Task<JsonResult> SexPercent()
        {
            List<string> title = new List<string>();
            List<int> value = new List<int>();
            int man = await _repositoryFactory.IMemberCard.CountAsync(m => m.Sex == "男");
            if (man > 0)
            {
                title.Add("男");
                value.Add(man);
            }
            int famale = await _repositoryFactory.IMemberCard.CountAsync(m => m.Sex == "女");
            if (famale > 0)
            {
                title.Add("女");
                value.Add(famale);
            }
            int other = await _repositoryFactory.IMemberCard.CountAsync(m => m.Sex != "女" && m.Sex != "男");
            if (other > 0)
            {
                title.Add("未知");
                value.Add(other);
            }
            return Json(new TipMessage() { Status = true, MsgText = "获取数据成功！", Data = new { legend = title, value = value } }, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// 会员卡充值总额排行
        /// </summary>
        /// <returns></returns>
        [HttpPost, PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult ChargeTotalRank()
        {
            var data = _repositoryFactory.IMemberCard.Where(m => m.TotalMoney > 0).Select(m => new { m.CardNo, m.TotalMoney }).OrderByDescending(m => m.TotalMoney).Take(10).ToList();
            List<string> title = new List<string>();
            List<decimal> value = new List<decimal>();
            foreach (var item in data)
            {
                title.Add(item.CardNo);
                value.Add((decimal)item.TotalMoney);
            }
            return Json(new TipMessage() { Status = true, MsgText = "获取数据成功！", Data = new { legend = title, value = value } }, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// 会员卡消费次数排行
        /// </summary>
        /// <returns></returns>
        [HttpPost, PermissionFilterAttribute(false, EnumHepler.ActionPermission.View)]
        public ActionResult ConsumeCountRank()
        {
            var list = (from db in new Entity.SmsEntities().CardHistory
                        where db.Type == 2
                        group db by db.CardNo into g
                        orderby g.Count() descending
                        select new { CradNo = g.Key, Count = g.Count() }).Take(10).ToList();
            List<string> title = new List<string>();
            List<decimal> value = new List<decimal>();
            foreach (var item in list)
            {
                title.Add(item.CradNo);
                value.Add(item.Count);
            }
            return Json(new TipMessage() { Status = true, MsgText = "获取数据成功！", Data = new { legend = title, value = value } }, JsonRequestBehavior.DenyGet);
        }
        #endregion
    }
}