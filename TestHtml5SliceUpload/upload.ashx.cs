using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace TestHtml5SliceUpload
{
    /// <summary>
    /// upload 的摘要说明
    /// </summary>
    public class upload : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/json";
            string name = context.Request["name"];
            int total = Convert.ToInt32(context.Request["total"]);
            int index = Convert.ToInt32(context.Request["index"]);
            long size = Convert.ToInt64(context.Request["size"]);
            var data = context.Request.Files["data"];
            string dir = context.Server.MapPath("~/upload/"+DateTime.Now.ToString("yyyy_MM_dd"));
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, name + "_" + index);
            data.SaveAs(file);

            if (index == total)
            {
                while (!CheckAllComplete(dir, name, total,size))
                {
                    System.Threading.Thread.Sleep(500);
                }
                CombineFile(dir, name, total);
                context.Response.Write("{\"error\":0,\"index\":" + index + ",\"complete\":1}");
            }
            else
            {
                context.Response.Write("{\"error\":0,\"index\":" + index + ",\"complete\":0}");
            }
        }

        bool CheckAllComplete(string dir, string name, int total,long size)
        {
            long flength = 0;
            for (int i = 1; i <= total; ++i)
            {
                try
                {
                    string part = Path.Combine(dir, name + "_" + i);
                    FileInfo finfo = new FileInfo(part);
                    flength += finfo.Length;
                }
                catch
                {
                    return false;
                }
            }
            if (flength != size) return false;
            return true;
        }

        void CombineFile(string dir, string name, int total) 
        {
            string file = Path.Combine(dir, name);
            var fs = new FileStream(file, FileMode.Create,FileAccess.ReadWrite,FileShare.Read);
            for (int i = 1; i <= total; ++i)
            {
                string part = Path.Combine(dir, name + "_" + i);
                var bytes = System.IO.File.ReadAllBytes(part);
                fs.Write(bytes, 0, bytes.Length);
                System.IO.File.Delete(part);
            }
            fs.Close();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}