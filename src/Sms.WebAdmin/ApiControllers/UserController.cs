using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Sms.WebAdmin.Models;

namespace Sms.WebAdmin.ApiControllers
{
    [AuthorizationAttribute]
    public class UserController : BaseController
    {
        [HttpPost, Route("api/user/login")]
        [AllowAnonymousAttribute]
        public HttpResponseMessage Login(string code)
        {
            return ApiResponse(new ApiResponseMessage() { Data = CurrentUser, Status = ResultStatus.Success });
        }

        [HttpPost, Route("api/user/register")]
        [AllowAnonymousAttribute]
        public HttpResponseMessage Register(string code)
        {
            return ApiResponse(new ApiResponseMessage() { Data = CurrentUser, Status = ResultStatus.Success });
        }

        [Route("api/user/getinfo")]
        public HttpResponseMessage GetUserInfo()
        {
            return ApiResponse(new ApiResponseMessage() { Data = CurrentUser, Status = ResultStatus.Success });
        }
    }
}
