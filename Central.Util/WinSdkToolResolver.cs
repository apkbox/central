using System;
using System.Collections.Generic;
namespace Central.Util
{
    using System.IO;
    using System.Linq;
    using System.Text;

    public class WinSdkToolResolver
    {
        private static readonly string SrcToolExe = @"srcsrv\srctool.exe";
        private static readonly string PdbStrExe = @"srcsrv\pdbstr.exe";
        private static string SymstoreExe = @"symstore.exe";

        public static string GetPath(WinSdkTool tool)
        {
            var path = GetSdkPath();
            if (path == null)
            {
                return null;
            }

            path = Path.Combine(path, @"Debuggers\");
            path = Path.Combine(path, Environment.Is64BitOperatingSystem ? @"x64" : @"x86");

            switch (tool)
            {
                case WinSdkTool.SrcTool:
                    path = Path.Combine(path, SrcToolExe);
                    break;
                case WinSdkTool.PdbStr:
                    path = Path.Combine(path, PdbStrExe);
                    break;
                case WinSdkTool.SymStore:
                    path = Path.Combine(path, SymstoreExe);
                    break;
            }

            return File.Exists(path) ? path : null;
        }

        private static string GetSdkPath()
        {
            var programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var windowsKits = Path.Combine(programFilesPath, @"Windows Kits");

            foreach (var v in new[] { @"8.1", @"8.0" })
            {
                var wdk = Path.Combine(windowsKits, v);
                if (Directory.Exists(wdk))
                {
                    return wdk;
                }
            }

            return null;
        }
    }
}
