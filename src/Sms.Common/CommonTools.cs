using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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
        /// 获取时间戳  
        /// </summary>  
        /// <returns></returns>  
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary> 
        /// 获取物理路径
        /// </summary> 
        /// <param name="strPath">虚拟路径</param> 
        public static string GetMapPath(string strPath)
        {
            return HttpContext.Current.Server.MapPath(strPath);
        }

        /// <summary>
        /// HTTP GET方式请求数据.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <returns></returns>
        public static string HttpGet(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            //request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "*/*";
            request.Proxy = null;
            request.Timeout = 15000;
            request.AllowAutoRedirect = false;

            WebResponse response = null;
            string responseStr = null;

            try
            {
                response = request.GetResponse();

                if (response != null)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    responseStr = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                request = null;
                response = null;
            }
            return responseStr;
        }

        /// <summary>
        /// 后台发送POST请求
        /// </summary>
        /// <param name="url">服务器地址</param>
        /// <param name="data">发送的数据</param>
        /// <returns></returns>
        public static Stream HttpPost(string url, string data)
        {
            //创建post请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json;charset=UTF-8";
            byte[] payload = Encoding.UTF8.GetBytes(data);
            request.ContentLength = payload.Length;


            //发送post的请求
            Stream writer = request.GetRequestStream();
            writer.Write(payload, 0, payload.Length);
            writer.Close();

            //接受返回来的数据
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            return stream;
            //StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            //string value = reader.ReadToEnd();

            //reader.Close();
            //stream.Close();
            //response.Close();
        }

        /// <summary>
        /// 后台发送POST请求
        /// </summary>
        /// <param name="url">服务器地址</param>
        /// <param name="data">发送的数据</param>
        /// <returns></returns>
        public static string HttpPostAndReturnString(string url, string data)
        {
            try
            {
                //创建post请求
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json;charset=UTF-8";
                byte[] payload = Encoding.UTF8.GetBytes(data);
                request.ContentLength = payload.Length;


                //发送post的请求
                Stream writer = request.GetRequestStream();
                writer.Write(payload, 0, payload.Length);
                writer.Close();

                //接受返回来的数据
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                string value = reader.ReadToEnd();

                reader.Close();
                stream.Close();
                response.Close();

                return value;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 解析小程序加密的数据
        /// </summary>
        /// <param name="encryptedDataStr"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string MiniProgAES_decrypt(string encryptedDataStr, string key, string iv)
        {
            RijndaelManaged rijalg = new RijndaelManaged();
            //-----------------    
            //设置 cipher 格式 AES-128-CBC    

            rijalg.KeySize = 128;

            rijalg.Padding = PaddingMode.PKCS7;
            rijalg.Mode = CipherMode.CBC;

            rijalg.Key = Convert.FromBase64String(key);
            rijalg.IV = Convert.FromBase64String(iv);


            byte[] encryptedData = Convert.FromBase64String(encryptedDataStr);
            //解密    
            ICryptoTransform decryptor = rijalg.CreateDecryptor(rijalg.Key, rijalg.IV);

            string result;

            using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        result = srDecrypt.ReadToEnd();
                    }
                }
            }

            return result;
        }
    }
}
