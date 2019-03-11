using Sms.Common;
using Sms.Entity.ViewModel;
using Sms.WebAdmin.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sms.WebAdmin.Controllers
{
    public class FileHandlerController : BaseController
    {
        [HttpPost, PermissionFilterAttribute(false, EnumHepler.ActionPermission.UpImage)]
        public ActionResult Image()
        {
            var _upfile = Request.Files["filedata"];//文件上传流      
            if (_upfile != null && _upfile.ContentLength < 2048000)
            {
                string ext = System.IO.Path.GetExtension(_upfile.FileName);
                string[] allowExt = { ".jpg", ".png", ".bmp", ".jpeg" };
                if (allowExt.Contains(ext))
                {
                    Random r = new Random();
                    string FileName = DateTime.Now.ToString("yyyyMMddHHmmss") + r.Next(100) + ext;
                    //保存路径
                    string path = "/Upload/Image/";
                    //分级目录
                    string childPath = DateTime.Now.ToString("yyyyMM") + "/";
                    //创建目录
                    FileDirectoryHelper.CreateDirectory(Server.MapPath(path + childPath));
                    //保存图片
                    string file = string.Format("{0}{1}{2}", path, childPath, FileName);
                    _upfile.SaveAs(Server.MapPath(file));
                    return Json(new TipMessage() { Status = true, MsgText = "上传成功", Url = file }, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    return Json(new TipMessage() { Status = false, MsgText = "请上传格式为jpg,jpeg,bmp,png的图片文件" }, JsonRequestBehavior.DenyGet);
                }
            }
            else
            {
                return Json(new TipMessage() { Status = false, MsgText = "文件最大为2M" }, JsonRequestBehavior.DenyGet);
            }
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