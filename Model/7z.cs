using SevenZip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unreal_GUI.Model
{
    class _7z
    {
        public static void ConfigureSevenZip()
        {
            // 从App目录加载
            SevenZipBase.SetLibraryPath(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "App", "7zxa.dll"));
        }
    }
}
