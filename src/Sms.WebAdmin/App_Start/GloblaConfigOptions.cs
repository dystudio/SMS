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
        }

        public string MiniProgramAppId { get; set; }

        public string MiniProgramAppSecret { get; set; }
    }
}