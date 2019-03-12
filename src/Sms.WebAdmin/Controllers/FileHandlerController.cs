using Sms.Common;
using Sms.Entity.ViewModel;
using Sms.WebAdmin.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Sms.WebAdmin.Controllers
{
    public class FileHandlerController : BaseController
    {
        [HttpPost, PermissionFilterAttribute(false, EnumHepler.ActionPermission.UpImage)]
        public ActionResult Image()
        {
            List<string> url = new List<string>();
            for (int i = 0; i < Request.Files.Count; i++)
            {
                var _upfile = Request.Files[i];//文件上传流      
                string FileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(_upfile.FileName);
                //保存路径
                string path = "/Upload/Image/";
                //分级目录
                string childPath = DateTime.Now.ToString("yyyyMM") + "/";
                //创建目录
                FileDirectoryHelper.CreateDirectory(Server.MapPath(path + childPath));
                //保存图片
                string file = string.Format("{0}{1}{2}", path, childPath, FileName);
                _upfile.SaveAs(Server.MapPath(file));
                url.Add(file);
            }
            if (url.Any())
            {
                return Json(new TipMessage() { Status = true, MsgText = "上传成功", Url = string.Join("$", url) }, JsonRequestBehavior.DenyGet);
            }
            return Json(new TipMessage() { Status = false, MsgText = "没有要上传的文件" }, JsonRequestBehavior.DenyGet);
        }

        public ActionResult DownLoadFile(string path, string content)
        {
            //下载到客户端
            System.IO.FileStream reader = System.IO.File.OpenRead(path);
            //文件传送的剩余字节数：初始值为文件的总大小
            long length = reader.Length;
            Response.Buffer = false;
            Response.AddHeader("Connection", "Keep-Alive");
            Response.ContentType = content;
            Response.Charset = "utf-8";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + System.IO.Path.GetFileName(path));
            Response.AddHeader("Content-Length", length.ToString());
            byte[] buffer = new Byte[10000];        //存放欲发送数据的缓冲区
            int byteToRead;                                         //每次实际读取的字节数
            while (length > 0)
            {
                //剩余字节数不为零，继续传送
                if (Response.IsClientConnected)
                {
                    //客户端浏览器还打开着，继续传送
                    byteToRead = reader.Read(buffer, 0, 10000);                 //往缓冲区读入数据
                    Response.OutputStream.Write(buffer, 0, byteToRead); //把缓冲区的数据写入客户端浏览器
                    Response.Flush();                                                                       //立即写入客户端
                    length -= byteToRead;                                                               //剩余字节数减少
                }
                else
                {
                    //客户端浏览器已经断开，阻止继续循环
                    length = -1;
                }
            }
            //关闭该文件
            reader.Close();
            //删除服务器上的该Excel文件
            System.IO.File.Delete(path);
            return new EmptyResult();
            //return File(path, content,System.IO.Path.GetFileName(path));
        }
    }
}