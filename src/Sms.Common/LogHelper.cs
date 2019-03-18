using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.Common
{
    public class LogHelper
    {
        /// <summary>
        /// 异常日志信息
        /// </summary>
        /// <param name="strMessage">异常日志信息</param>
        public static void Exception(string strMessage)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("Exception");
            if (log.IsErrorEnabled)
            {
                log.Error(strMessage);
            }
            log = null;
        }
        public static void Exception(Exception ex)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("Exception");
            if (log.IsErrorEnabled)
            {
                log.Error(ex.Message, ex);
            }
            log = null;
        }

        /// <summary>
        /// 普通的日志信息
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void Info(string message)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("InfoLog");
            if (log.IsInfoEnabled)
            {
                log.Info(message);
            }
            log = null;
        }


        /// <summary>
        /// 支付日志信息
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void Payment(string orderCode, string message)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("Payment");
            if (log.IsInfoEnabled)
            {
                log.Info($"[单据号-{orderCode}]：{message}");
            }
            log = null;
        }

        /// <summary>
        /// 处理结果日志
        /// </summary>
        /// <param name="strPage">请求页面</param>
        /// <param name="strName">请求者</param>
        /// /// <param name="strIp">请求IP</param>
        /// <param name="strResult">处理结果</param>
        public static void ResultLog(string strPage, string strName, string strIp, string strResult)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("Result");
            if (log.IsInfoEnabled)
            {
                string strMessage = string.Format("请求页面:{0}\r\n请求者:{1}\r\n请求IP:{2}\r\n处理结果:{3}", strPage, strName, strIp, strResult);
                log.Info(strMessage);
            }
            log = null;
        }
    }
}
