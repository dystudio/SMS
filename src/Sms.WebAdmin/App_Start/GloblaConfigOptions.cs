using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Sms.WebAdmin
{
    public class GloblaConfigOptions
    {
        public static GloblaConfigOptions Instance { get; private set; }

        public static void Init()
        {
            Instance = new GloblaConfigOptions();
            Instance.MiniProgramAppId = ConfigurationManager.AppSettings["MiniProgramAppId"];
            Instance.MiniProgramAppSecret = ConfigurationManager.AppSettings["MiniProgramAppSecret"];
            Instance.WxPayMerchantId = ConfigurationManager.AppSettings["WxPayMerchantId"];
            Instance.WxPayMerchantKey = ConfigurationManager.AppSettings["WxPayMerchantKey"];
        }

        public string MiniProgramAppId { get; set; }

        public string MiniProgramAppSecret { get; set; }


        public string WxPayMerchantId { get; set; }

        public string WxPayMerchantKey { get; set; }
    }
}