namespace Central.SrcSrv
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography;

    public class SourceStore
    {
        private static readonly string SrcToolExe = @"C:\Program Files (x86)\Windows Kits\8.1\Debuggers\x64\srcsrv\srctool.exe";
        private static readonly string PdbStrExe = @"C:\Program Files (x86)\Windows Kits\8.1\Debuggers\x64\srcsrv\pdbstr.exe";

        public string SourceStoreDirectory { get; set; }

        static SourceStore()
        {
            SrcToolExe = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                @"Windows Kits\8.1\Debuggers\x86\srcsrv\srctool.exe");
            PdbStrExe = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                @"Windows Kits\8.1\Debuggers\x86\srcsrv\pdbstr.exe");
        }

        public SourceStore()
        {
        }

        public static string GetRelativeStorePath(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentException("File path cannot be empty", "filePath");
            }

            string fileHash = GetFileHash(filePath);
            string fileName = Path.GetFileName(filePath);

            return Path.Combine(Path.Combine(fileName, fileHash), fileName);
        }

        public static string GetFileHash(string filePath)
        {
            string fileHash;
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sha1 = new SHA1Managed())
                {
                    fileHash = BitConverter.ToString(sha1.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
                }
            }
            return fileHash;
        }

        public string GetStorePath(string filePath)
        {
            if (this.SourceStoreDirectory == null)
            {
                throw new InvalidOperationException("SourceStoreDirectory not specified.");
            }

            if (filePath == null)
            {
                throw new ArgumentException("File path cannot be empty", "filePath");
            }

            string fileHash;

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sha1 = new SHA1Managed())
                {
                    fileHash = BitConverter.ToString(sha1.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
                }
            }

            string fileStoreDirectory = Path.Combine(
                Path.Combine(this.SourceStoreDirectory, Path.GetFileName(filePath)),
                fileHash);

            return Path.Combine(fileStoreDirectory, Path.GetFileName(filePath));
        }

        public string AddFile(string filePath)
        {
            var storeFilePath = this.GetStorePath(filePath);
            var storeFileDirectory = Path.GetDirectoryName(storeFilePath);

            Directory.CreateDirectory(storeFileDirectory);

            // TODO: Note that just like in symbol store we have to store
            // a record if file come from different location.
            if (!File.Exists(storeFilePath))
            {
                File.Copy(filePath, storeFilePath, true);
            }

            return storeFilePath;
        }

        public static List<string> GetSourceFilesFromPdb(string pdbFile)
        {
            var sourceFiles = new List<string>();

            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = SrcToolExe;
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
            process.StartInfo.FileName = PdbStrExe;
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
