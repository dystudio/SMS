using Sms.WebAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Senparc.Weixin.TenPay.V3;
using Newtonsoft.Json;
using Sms.Common;

namespace Sms.WebAdmin.ApiControllers
{
    [AuthorizationAttribute]
    public class PaymentController : BaseController
    {
        private string appId => GloblaConfigOptions.Instance.MiniProgramAppId;
        private string appSecrect => GloblaConfigOptions.Instance.MiniProgramAppSecret;
        private string wxmchId => GloblaConfigOptions.Instance.WxPayMerchantId;
        private string wxmchKey => GloblaConfigOptions.Instance.WxPayMerchantKey;
        //异步通知地址
        private string notifyUrl => $"{Request.RequestUri.Scheme}://{Request.RequestUri.Host}:{Request.RequestUri.Port}/api/wxpay/notify";

        /// <summary>
        /// 创建微信支付请求
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        [HttpPost, Route("api/wxpay/create")]
        public HttpResponseMessage CreatePayment(string orderCode)
        {
            LogHelper.Payment(orderCode, "准备请求微信支付接口...");
            var orderEntity = _repositoryFactory.IOrders.Single(x => x.OrderCode == orderCode);
            if (orderEntity == null)
            {
                LogHelper.Payment(orderCode, "订单不存在");
                return ApiResponse(ResultStatus.ParamError, "订单不存在");
            }
            if (orderEntity.OrderStatus != (int)EnumHepler.OrderStatus.Created || orderEntity.PayStatus != (int)EnumHepler.OrderPayStatus.Unpay)
            {
                LogHelper.Payment(orderCode, "订单非待支付状态");
                return ApiResponse(ResultStatus.ParamError, "订单非待支付状态");
            }
            TenPayV3Info TenPayV3Info = new TenPayV3Info(appId, appSecrect, wxmchId, wxmchKey, notifyUrl, notifyUrl);
            TenPayV3Info.TenPayV3Notify = notifyUrl;
            //创建支付应答对象
            //RequestHandler packageReqHandler = new RequestHandler(null);
            var sp_billno = DateTime.Today.ToString("yyMMdd") + TenPayV3Util.BuildDailyRandomStr(4);//最多32位
            var nonceStr = TenPayV3Util.GetNoncestr();
            string clientIp = CommonTools.GetIpAddress();
            //创建请求统一订单接口参数
            var xmlDataInfo = new TenPayV3UnifiedorderRequestData(TenPayV3Info.AppId, TenPayV3Info.MchId, "订单支付", orderCode, (int)(orderEntity.ActualPrice * 100), clientIp, TenPayV3Info.TenPayV3Notify, Senparc.Weixin.TenPay.TenPayV3Type.JSAPI, orderEntity.OpenId, TenPayV3Info.Key, nonceStr);

            //返回给微信的请求
            //RequestHandler res = new RequestHandler(null);
            try
            {
                //调用统一订单接口
                var result = TenPayV3.Unifiedorder(xmlDataInfo);
                LogHelper.Payment(orderCode, $"微信支付接口返回：{JsonConvert.SerializeObject(result)}");

                if (result != null && result.return_code == "SUCCESS")
                {
                    //return_code是通信标识，非交易标识，交易是否成功需要查看result_code来判断
                    if (result.result_code == "SUCCESS")
                    {
                        LogHelper.Payment(orderCode, $"请求微信支付成功，预支付单号：{result.prepay_id}");
                        string rtimeStamp = CommonTools.GetTimeStamp();
                        //string nativeReqSign = res.CreateMd5Sign("key", TenPayV3Info.Key);
                        string paySign = SecurityHelper.MD5($"appId={TenPayV3Info.AppId}&nonceStr={nonceStr}&package=prepay_id={result.prepay_id}&signType=MD5&timeStamp={rtimeStamp}&key={TenPayV3Info.Key}").ToUpper();
                        //返回前端唤起微信支付收银台的参数
                        var detail = new { timeStamp = rtimeStamp, nonceStr = nonceStr, package = $"prepay_id={result.prepay_id}", signType = "MD5", paySign = paySign };
                        return ApiResponse(ResultStatus.Success, "下单成功", detail);
                    }
                    else
                    {
                        LogHelper.Payment(orderCode, $"请求微信支付失败，err_code：{result.err_code}，err_code_des：{result.err_code_des}");
                    }
                }
                return ApiResponse(ResultStatus.Failed, result.return_msg);
            }
            catch (Exception ex)
            {
                //res.SetParameter("return_code", "FAIL");
                //res.SetParameter("return_msg", "统一下单失败");
                LogHelper.Payment(orderCode, $"创建微信支付异常：{ex.Message}");
                LogHelper.Exception(ex);
                return ApiResponse(ResultStatus.Failed, "创建微信支付请求失败，请联系管理员！");
            }

        }

        /// <summary>
        /// 接受来自微信回调的支付结果
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("api/wxpay/notify")]
        public IHttpActionResult WechatNotify()
        {
            string reqKey = TenPayV3Util.BuildRandomStr(8);
            LogHelper.Payment(reqKey, $"收到微信支付异步通知，准备解析结果...");
            try
            {
                ResponseHandler resHandler = new ResponseHandler(null);
                string return_code = resHandler.GetParameter("return_code");
                string return_msg = resHandler.GetParameter("return_msg");
                LogHelper.Payment(reqKey, $"解析返回结果为return_code：{return_code}，return_msg：{return_msg}");
                LogHelper.Info($"[{reqKey}]{resHandler.ParseXML()}");

                TenPayV3Info TenPayV3Info = new TenPayV3Info(appId, appSecrect, wxmchId, wxmchKey, notifyUrl, notifyUrl);
                resHandler.SetKey(TenPayV3Info.Key);
                //验证请求是否从微信发过来（安全）
                if (resHandler.IsTenpaySign() && return_code.ToUpper() == "SUCCESS")
                {
                    //return_code是通信标识，非交易标识，交易是否成功需要查看result_code来判断
                    if (resHandler.GetParameter("result_code") == "SUCCESS")
                    {
                        string total_fee = resHandler.GetParameter("total_fee");//订单金额
                        string out_trade_no = resHandler.GetParameter("out_trade_no");//商户订单号
                        string transaction_id = resHandler.GetParameter("transaction_id");//微信支付订单号
                        string time_end = resHandler.GetParameter("time_end");//支付完成时间
                        string openid = resHandler.GetParameter("openid");
                        LogHelper.Payment(reqKey, $"微信支付成功，商户订单号：{out_trade_no}，支付金额：{total_fee}，支付流水号：{transaction_id}，openid：{openid}");
                        //1.判断订单是否存在 2.判断订单支付状态 3.插入修改数据等
                        var orderEntity = _repositoryFactory.IOrders.Single(x => x.OrderCode == out_trade_no);
                        if (orderEntity == null)
                        {
                            LogHelper.Payment(reqKey, $"查找订单失败，商户订单号：{out_trade_no}");
                        }
                        else if (orderEntity.OrderStatus != (int)EnumHepler.OrderStatus.Created || orderEntity.PayStatus != (int)EnumHepler.OrderPayStatus.Unpay)
                        {
                            LogHelper.Payment(reqKey, $"订单非待支付状态，当前状态为{orderEntity.OrderStatus}，商户订单号：{out_trade_no}");
                        }
                        else
                        {
                            LogHelper.Payment(reqKey, $"更新订单支付状态为已支付待发货，商户订单号：{out_trade_no}");
                            DateTime payTime = DateTime.ParseExact(time_end, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.NoCurrentDateDefault);
                            //更新订单状态
                            _repositoryFactory.IOrders.ModifyBy(x => x.OrderCode == out_trade_no,
                                new string[] { "OrderStatus", "PayStatus", "PayTime", "PayMode" },
                                new object[] { (int)EnumHepler.OrderStatus.WaitSendGoods, (int)EnumHepler.OrderPayStatus.Paied, payTime, (int)EnumHepler.OrderPayMode.WechatPay });
                            _repositoryFactory.SaveChanges();
                        }
                    }
                    else
                    {
                        LogHelper.Payment(reqKey, $"微信支付失败，err_code：{resHandler.GetParameter("err_code")}，err_code_des：{resHandler.GetParameter("err_code_des")}");
                    }
                }

                string xml = string.Format(@"<xml>
<return_code><![CDATA[{0}]]></return_code>
<return_msg><![CDATA[{1}]]></return_msg>
</xml>", return_code, return_msg);
                return Content(HttpStatusCode.OK, xml, Configuration.Formatters.XmlFormatter, "text/xml");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
