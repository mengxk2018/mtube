using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Video
{
    public class Video : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var response = context.Response;
            var request = context.Request;
            var result = "";
            var type = request["type"] ?? "";
            switch (type)
            {
                case "load":
                    result = GetFiles();
                    break;
                case "delete":
                    var deleteFile = request["file"] ?? "";
                    result = DeleteFile(deleteFile);
                    break;
                case "save":
                    var name = request["name"] ?? "";
                    var poster = request["poster"] ?? "";
                    var info = request["info"] ?? "";
                    result = SaveInfo(name, poster, info);
                    break;
            }
            response.Write(result);
            response.End();
        }

        private string GetFiles()
        {
            var result = "";
            try
            {
                var source = Sources;
                var files = Directory.GetFiles(source, "*.*", SearchOption.AllDirectories);
                files = files.Where(file => file.EndsWith(".mp4", StringComparison.CurrentCultureIgnoreCase) || file.EndsWith(".mkv", StringComparison.CurrentCultureIgnoreCase) || file.EndsWith(".rmvb", StringComparison.CurrentCultureIgnoreCase)).OrderBy(file => new FileInfo(file).LastWriteTime).Reverse().ToArray();
                result = files.ToJson();
            }
            catch { }
            return result;
        }

        private string DeleteFile(string file)
        {
            var result = "";
            try
            {
                var delete = HttpContext.Current.Request.Cookies["delete"];
                if (delete == null) return result;

                var path = Path.Combine(Sources, file);
                if (File.Exists(path))
                {
                    File.Delete(path);
                    result = "success";
                }
            }
            catch { }
            return result;
        }

        private string SaveInfo(string name, string poster, string info)
        {
            var result = "";
            try
            {
                var response = WebRequest.CreateHttp(poster).GetResponse();
                var stream = response.GetResponseStream();
                var image = Image.FromStream(stream);
                var folder = HttpContext.Current.Server.MapPath("/data");

                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, name + ".json"), info);
                image.Save(Path.Combine(folder, name + ".jpg"));

                stream.Close();
                response.Close();

                result = "success";
            }
            catch { }
            return result;
        }

        public string Sources
        {
            get { return ConfigurationManager.AppSettings["source"]; }
        }

        public string Player
        {
            get { return ConfigurationManager.AppSettings["player"]; }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    public static class JsonHelper
    {
        public static string ToJson<T>(this T data)
        {
            var json = JsonConvert.SerializeObject(data);
            return json;
        }

        public static T ToObject<T>(this string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}