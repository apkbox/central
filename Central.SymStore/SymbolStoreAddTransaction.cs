namespace Central.SymStore
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    public class SymbolStoreAddTransaction : SymbolStoreTransaction
    {
        private readonly List<string> files = new List<string>();

        private const string SymstoreExe = @"C:\Program Files (x86)\Windows Kits\8.1\Debuggers\x64\symstore.exe";

        public bool Compress { get; set; }

        public string Comment { get; set; }

        public string Product { get; set; }
        public string Version { get; set; }

        public void AddFile(string file)
        {
            this.files.Add(Path.GetFullPath(file));
        }

        internal override void Commit(string symbolStoreDirectory)
        {
            var commonPath = Util.GetCommonPath(this.files);
            var indexFile = Path.GetTempFileName();
            this.GenerateIndex(commonPath, indexFile);
            this.AddToStore(commonPath, indexFile, symbolStoreDirectory);
        }

        private void AddToStore(string commonPath, string indexFile, string symbolStoreDirectory)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = SymstoreExe;
            process.StartInfo.Arguments = this.CreateYCommandLine(commonPath, indexFile, symbolStoreDirectory);
            // process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
        }

        private void GenerateIndex(string commonPath, string indexFile)
        {
            foreach (var file in this.files)
            {
                var process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = SymstoreExe;
                process.StartInfo.Arguments = this.CreateXCommandLine(commonPath, file, indexFile);
                // process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
            }
        }

        private string CreateXCommandLine(string commonPath, string file, string indexFile)
        {
            var commandLine = new List<string>();

            commandLine.Add("add");
            commandLine.Add("/o");
            commandLine.Add("/a");
            commandLine.Add(string.Format("/x \"{0}\"", indexFile));
            commandLine.Add(string.Format("/g \"{0}\"", commonPath));
            commandLine.Add(string.Format("/f \"{0}\"", file));

            return string.Join(" ", commandLine.ToArray());
        }

        private string CreateYCommandLine(string commonPath, string indexFile, string storeDirectory)
        {
            var commandLine = new List<string>();

            commandLine.Add("add");
            commandLine.Add("/o");
            commandLine.Add(string.Format("/s \"{0}\"", storeDirectory));

            if (this.Comment.Length > 0)
            {
                commandLine.Add(string.Format("/c \"{0}\"", this.Comment.Replace("\"", "\\\"")));
            }

            if (this.Product.Length > 0)
            {
                commandLine.Add(string.Format("/t \"{0}\"", this.Product.Replace("\"", "\\\"")));
            }

            if (this.Version.Length > 0)
            {
                commandLine.Add(string.Format("/v \"{0}\"", this.Version.Replace("\"", "\\\"")));
            }

            commandLine.Add(string.Format("/y \"{0}\"", indexFile));
            commandLine.Add(string.Format("/g \"{0}\"", commonPath));

            return string.Join(" ", commandLine.ToArray());
        }
    }
}