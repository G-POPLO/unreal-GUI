using SevenZip;

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
