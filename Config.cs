using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace unreal_GUI
{
    internal class Config
    {
        public static void CreateConfig()
        {
            if (!File.Exists("config.ini"))
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
                File.WriteAllText("config.ini", $"[Version]\nCurrent={version}");
            }
        }
    }
}
