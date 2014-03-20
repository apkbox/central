namespace Central.SymStore
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    public class AddDirectoryTransaction : AddTransactionBase
    {
        public AddDirectoryTransaction()
        {
        }

        public AddDirectoryTransaction(string product)
        {
            this.Product = product;
        }

        public string commonPath;

        /// <summary>
        /// The add files to the transaction recursively.
        /// </summary>
        /// <param name="directory">
        /// The full path to the directory.
        /// </param>
        public void AddDirectory(string directory)
        {
            this.commonPath = directory;
            this.GenerateIndexFromDirectory(directory, this.IndexFile);
        }

        internal override void Commit(string symbolStoreDirectory)
        {
            if (string.IsNullOrEmpty(this.Product))
            {
                throw new InvalidOperationException("Product property must be a non empty string.");
            }

            this.AddToStore(this.commonPath, this.IndexFile, symbolStoreDirectory);
        }

        private void GenerateIndexFromDirectory(string directory, string indexFile)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = SymstoreExe;
            process.StartInfo.Arguments = this.CreateRecursiveXCommandLine(directory, indexFile);
            // process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
        }
    }
}