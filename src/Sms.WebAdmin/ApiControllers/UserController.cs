using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Sms.WebAdmin.Models;
using System.Configuration;
using Sms.Common;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.IO;

namespace Sms.WebAdmin.ApiControllers
{
    [AuthorizationAttribute]
    public class UserController : BaseController
    {
        /// <summary>
        /// 微信登录
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous, HttpPost, Route("api/user/login")]
        public HttpResponseMessage Login(string code)
        {
            string appid = GloblaConfigOptions.Instance.MiniProgramAppId;
            string appsecrect = GloblaConfigOptions.Instance.MiniProgramAppSecret;
            //请求地址
            string url = $"https://api.weixin.qq.com/sns/jscode2session?appid={appid}&secret={appsecrect}&js_code={code}&grant_type=authorization_code";
            string result = CommonTools.HttpGet(url);
            //反序列化为json
            dynamic json = JToken.Parse(result) as dynamic;
            if (json == null || (json.errcode != null && json.errcode != 0))
            {
                return ApiResponse(new ApiResponseMessage() { Message = $"{json.errcode}:{json.errmsg}。code:{code}", Status = ResultStatus.Failed });
            }
            string openid = json.openid;
            string securityOpenId = SecurityHelper.EncryptDES(openid, Common.ConstFiled.OpenIdEncryptKey);
            var user = _repositoryFactory.IWeChatMember.Single(x => x.OpenId == openid);
            if (user == null)
            {
                //未注册的话前端要引导用户去授权登录，授权完再调用注册接口，根据返回状态去判断
                return ApiResponse(new ApiResponseMessage() { Message = "微信用户未注册", Data = securityOpenId, Status = ResultStatus.UnAuthorize });
            }
            _repositoryFactory.IWeChatMember.ModifyBy(x => x.OpenId == openid, new string[] { "SessionKey" }, new object[] { json.session_key.Value });
            _repositoryFactory.SaveChanges();
            return ApiResponse(new ApiResponseMessage() { Data = new { openid = securityOpenId }, Status = ResultStatus.Success });
        }

        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="jdata">{openid,userInfo:{avatarUrl,city,country,gender,nickName,province}}</param>
        /// <returns></returns>
        [AllowAnonymous, HttpPost, Route("api/user/register")]
        public HttpResponseMessage Register([FromBody]JObject jdata)
        {
            dynamic args = jdata;
            WXAppUserInfo info = (args.userInfo as JObject).ToObject<WXAppUserInfo>();
            if (info == null)
            {
                return ApiResponse(new ApiResponseMessage() { Message = "用户信息不能为空", Status = ResultStatus.Failed });
            }
            string openid = SecurityHelper.DecryptDES(args.openid.Value, Common.ConstFiled.OpenIdEncryptKey);
            var user = _repositoryFactory.IWeChatMember.Single(x => x.OpenId == openid);
            bool modify = true;
            if (user == null)
            {
                modify = false;
                user = new Entity.WeChatMember();
            }
            user.AvatarUrl = info.avatarUrl;
            user.City = info.city;
            user.Country = info.country;
            user.Gender = info.gender;
            user.NickName = info.nickName;
            user.Province = info.province;
            if (modify)
            {
                _repositoryFactory.IWeChatMember.Modify(user, "NickName", "AvatarUrl", "Gender", "Country", "Province", "City");
            }
            else
            {
                user.OpenId = openid;
                user.CreateTime = DateTime.Now;
                user.Status = 1;
                _repositoryFactory.IWeChatMember.Add(user);
            }
            _repositoryFactory.SaveChanges();
            return ApiResponse(new ApiResponseMessage() { Data = info, Status = ResultStatus.Success });
        }

        /// <summary>
        /// 获取登录的用户信息
        /// </summary>
        /// <returns></returns>
        [Route("api/user/getinfo")]
        public HttpResponseMessage GetUserInfo()
        {
            return ApiResponse(new ApiResponseMessage() { Data = CurrentUser, Status = ResultStatus.Success });
        }

        /// <summary>
        /// 绑定手机号
        /// </summary>
        /// <param name="jdata">{encryptedData,iv}</param>
        /// <returns></returns>
        [HttpPost, Route("api/user/bindmobile")]
        public HttpResponseMessage BindMobile([FromBody]JObject jdata)
        {
            dynamic args = jdata;
            string openid = CurrentUser.OpenId;
            var user = _repositoryFactory.IWeChatMember.Single(x => x.OpenId == openid);
            if (user == null)
            {
                //未注册的话前端要引导用户去授权登录，授权完再调用注册接口，根据返回状态去判断
                return ApiResponse(new ApiResponseMessage() { Message = "微信用户未注册", Status = ResultStatus.UnAuthorize });
            }
            string mobile = CommonTools.MiniProgAES_decrypt(args.encryptedData.Value, user.SessionKey, args.iv.Value);
            dynamic json = JToken.Parse(mobile) as dynamic;
            if (json != null && json.phoneNumber != null)
            {
                _repositoryFactory.IWeChatMember.ModifyBy(x => x.OpenId == openid, new string[] { "Mobile" }, new object[] { json.phoneNumber.Value });
                _repositoryFactory.SaveChanges();
                return ApiResponse(new ApiResponseMessage() { Message = "绑定成功", Status = ResultStatus.Success });
            }
            else
            {
                return ApiResponse(new ApiResponseMessage() { Message = "解析手机号失败", Status = ResultStatus.Failed });
            }

        }

    }
}
