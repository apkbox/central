namespace Central.SymStore
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    using Central.Util;

    public abstract class AddTransactionBase : SymbolStoreTransaction
    {
        private readonly TempScope tempScope = new TempScope();

        /// <summary>
        /// Initializes a new instance of the <see cref="AddTransactionBase"/> class.
        /// </summary>
        protected AddTransactionBase()
        {
            this.IndexFile = this.tempScope.GetUniqueName();
        }

        /// <summary>
        /// Gets or sets a value indicating whether to create a compressed 
        /// version of each file copied to the symbol store instead of using 
        /// an uncompressed copy of the file.
        /// </summary>
        public bool Compress { get; set; }

        /// <summary>
        /// Gets or sets the transaction comment.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the product name. This property must be set for the transaction to succeed.
        /// </summary>
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the product version.
        /// </summary>
        public string Version { get; set; }

        public string IndexFile { get; set; }

        protected string CreateRecursiveXCommandLine(string directory, string indexFile)
        {
            var commandLine = new List<string>();

            commandLine.Add("add");
            commandLine.Add("/o");
            commandLine.Add("/a");
            commandLine.Add(string.Format("/x \"{0}\"", indexFile));
            commandLine.Add(string.Format("/g \"{0}\"", directory));
            commandLine.Add(string.Format("/f \"{0}\"", directory));
            commandLine.Add("/r");

            return string.Join(" ", commandLine.ToArray());
        }

        protected string CreateXCommandLine(string commonPath, string file, string indexFile)
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

        protected string CreateYCommandLine(string commonPath, string indexFile, string storeDirectory)
        {
            var commandLine = new List<string>();

            commandLine.Add("add");
            commandLine.Add("/o");
            commandLine.Add(string.Format("/s \"{0}\"", storeDirectory));

            if (!string.IsNullOrEmpty(this.Comment))
            {
                commandLine.Add(string.Format("/c \"{0}\"", this.Comment.Replace("\"", "\\\"")));
            }

            if (!string.IsNullOrEmpty(this.Product))
            {
                commandLine.Add(string.Format("/t \"{0}\"", this.Product.Replace("\"", "\\\"")));
            }

            if (!string.IsNullOrEmpty(this.Version))
            {
                commandLine.Add(string.Format("/v \"{0}\"", this.Version.Replace("\"", "\\\"")));
            }

            commandLine.Add(string.Format("/y \"{0}\"", indexFile));
            commandLine.Add(string.Format("/g \"{0}\"", commonPath));

            return string.Join(" ", commandLine.ToArray());
        }

        protected void AddToStore(string commonPath, string indexFile, string symbolStoreDirectory)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = WinSdkToolResolver.GetPath(WinSdkTool.SymStore);
            process.StartInfo.Arguments = this.CreateYCommandLine(commonPath, indexFile, symbolStoreDirectory);
            // process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
        }
    }
}