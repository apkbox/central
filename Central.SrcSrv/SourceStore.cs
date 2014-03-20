namespace Central.SrcSrv
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography;

    public class SourceStore
    {
        private const string SrcToolExe = @"C:\Program Files (x86)\Windows Kits\8.1\Debuggers\x64\srcsrv\srctool.exe";

        public string SourceStoreDirectory { get; set; }

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
    }
}
