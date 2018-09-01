using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Sms.Common
{
    /// <summary>
    /// 通用的公共方法
    /// </summary>
    public class CommonTools
    {
        /// <summary>
        /// 获取客户端的IP地址
        /// </summary>
        /// <returns>IP地址</returns>
        public static String GetIpAddress()
        {
            string result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            if (string.IsNullOrEmpty(result))
            {
                result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            }
            if (string.IsNullOrEmpty(result))
            {
                result = HttpContext.Current.Request.UserHostAddress;
            }
            if (string.IsNullOrEmpty(result) || !Regex.IsMatch(result, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$"))
            {
                result = "127.0.0.1";
            }
            return result;
        }

        /// <summary>
        /// 判断客户端是否是移动终端
        /// </summary>
        /// <returns></returns>
        public static bool IsMobile()
        {
            if (HttpContext.Current == null
                || HttpContext.Current.Request == null
                || HttpContext.Current.Request.UserAgent == null) return false;

            var userAgent = HttpContext.Current.Request.UserAgent.ToLower();

            if (string.IsNullOrEmpty(userAgent)) return false;

            bool isAndroid = userAgent.Contains("android");

            bool isBlackBerry = userAgent.Contains("blackberry");

            bool isiOS = userAgent.Contains("iphone") || userAgent.Contains("ipad") || userAgent.Contains("ipod");

            bool isOperaMini = userAgent.Contains("opera mini");

            bool isWindowPhone = userAgent.Contains("wpdesktop");

            return isAndroid || isBlackBerry || isiOS || isWindowPhone;

        }


        /// <summary> 
        /// 获取物理路径
        /// </summary> 
        /// <param name="strPath">虚拟路径</param> 
        public static string GetMapPath(string strPath)
        {
            return HttpContext.Current.Server.MapPath(strPath);
        }
    }
}
