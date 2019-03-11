using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Sms.Entity;

namespace Sms.WebAdmin.ApiControllers
{
    public class ItemController : BaseController
    {
        public string[] Get()
        {
            return new string[] { "value1", "value2" };
        }
    }
}