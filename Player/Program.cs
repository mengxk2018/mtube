using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Player
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) return;

            try
            {
                var source = "";
                var player = "";
                var config = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web.config");
                if (File.Exists(config))
                {
                    var xml = new XmlDocument();
                    xml.Load(config);

                    var xnSource = xml.SelectSingleNode("//add[@key='source']");
                    if (xnSource != null) source = xnSource.Attributes["value"].Value;

                    var xnPlayer = xml.SelectSingleNode("//add[@key='player']");
                    if (xnPlayer != null) player = xnPlayer.Attributes["value"].Value;
                }

                var videoName = HttpUtility.UrlDecode(args[0].Replace("player://", "").Replace("movie/", "").Replace("/", ""));
                var video = Path.Combine(source, videoName);
                if (File.Exists(video))
                {
                    if (File.Exists(player))
                    {
                        Process.Start(player, video);
                    }
                    else
                    {
                        Process.Start(video);
                    }
                }
            }
            catch { }
        }
    }
}