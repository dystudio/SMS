using Sms.WebAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Sms.WebAdmin.ApiControllers
{
    [AuthorizationAttribute]
    public class PaymentController : BaseController
    {
        /// <summary>
        /// 创建订单并发起微信支付请求
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <param name="encryptedData"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        [HttpPost, Route("api/wxpay/create")]
        public HttpResponseMessage CreateOrderAndPayment(string sessionKey, string encryptedData, string iv)
        {
            return ApiResponse(new ApiResponseMessage() { Message = "绑定成功", Status = ResultStatus.Success });
        }

        /// <summary>
        /// 接受来自微信回调的支付结果
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <param name="encryptedData"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        [HttpPost, Route("api/wxpay/notify")]
        public HttpResponseMessage WechatNotify(string sessionKey, string encryptedData, string iv)
        {
            return ApiResponse(new ApiResponseMessage() { Message = "绑定成功", Status = ResultStatus.Success });
        }
    }
}
