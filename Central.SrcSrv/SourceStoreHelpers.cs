namespace Central.SrcSrv
{
    using Central.Util;

    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// The source store helpers.
    /// </summary>
    public class SourceStoreHelpers
    {
        public static List<string> GetSourceFilesFromPdb(string pdbFile)
        {
            var sourceFiles = new List<string>();

            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = WinSdkToolResolver.GetPath(WinSdkTool.SrcTool);
            process.StartInfo.Arguments = CreateSrcToolCommandLine(pdbFile);
            process.StartInfo.RedirectStandardOutput = true;
            // process.StartInfo.CreateNoWindow = true;
            process.Start();
            while (true)
            {
                var sourceFileReference = process.StandardOutput.ReadLine();
                if (sourceFileReference == null)
                {
                    break;
                }

                sourceFiles.Add(sourceFileReference);
            }

            process.WaitForExit();

            return sourceFiles;
        }

        private static string CreateSrcToolCommandLine(string pdbFile)
        {
            var commandLine = new List<string>();

            commandLine.Add("-r");
            commandLine.Add(string.Format("\"{0}\"", pdbFile));

            return string.Join(" ", commandLine.ToArray());
        }

        public static List<string> AddStreamToPdb(string pdbFile, string srcsrvIniFile)
        {
            var sourceFiles = new List<string>();

            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = WinSdkToolResolver.GetPath(WinSdkTool.PdbStr);
            process.StartInfo.Arguments = CreatePdbstrCommandLine(pdbFile, srcsrvIniFile);
            // process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();

            return sourceFiles;
        }

        private static string CreatePdbstrCommandLine(string pdbFile, string srcsrvIniFile)
        {
            var commandLine = new List<string>();

            commandLine.Add("-w");
            commandLine.Add(string.Format("\"-p:{0}\"", pdbFile));
            commandLine.Add(string.Format("\"-i:{0}\"", srcsrvIniFile));
            commandLine.Add("-s:srcsrv");

            return string.Join(" ", commandLine.ToArray());
        }
    }
}
