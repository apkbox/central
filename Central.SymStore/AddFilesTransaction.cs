namespace Central.SymStore
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    using Central.Util;

    public class AddFilesTransaction : AddTransactionBase
    {
        private readonly List<string> files = new List<string>();

        public AddFilesTransaction()
        {
        }

        public AddFilesTransaction(string product)
        {
            this.Product = product;
        }

        /// <summary>
        /// Adds file to the transaction.
        /// </summary>
        /// <param name="file">
        /// Full path to the file.
        /// </param>
        public void AddFile(string file)
        {
            var path = Path.GetFullPath(file);
            this.files.Add(path);
        }

        internal override void Commit(string symbolStoreDirectory)
        {
            if (string.IsNullOrEmpty(this.Product))
            {
                throw new InvalidOperationException("Product property must be a non empty string.");
            }

            if (this.files.Count == 0)
            {
                throw new InvalidOperationException("Nothing to add.");
            }

            var commonPath = PathUtil.GetCommonPath(this.files);
            this.GenerateIndex(commonPath, this.IndexFile);
            this.AddToStore(commonPath, this.IndexFile, symbolStoreDirectory);
        }

        private void GenerateIndex(string commonPath, string indexFile)
        {
            foreach (var file in this.files)
            {
                var process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = WinSdkToolResolver.GetPath(WinSdkTool.SymStore);
                process.StartInfo.Arguments = this.CreateXCommandLine(commonPath, file, indexFile);
                // process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
            }
        }
    }
}