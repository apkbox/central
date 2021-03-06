﻿namespace Central
{
    using System;
    using System.Collections.Generic;
    using System.Data.Odbc;
    using System.IO;
    using System.Linq;

    using Fclp;

    public class Parameters
    {
        public Parameters()
        {
            this.Sources = new List<SourceTreeRoot>();
            this.Binaries = new List<string>();
        }

        public List<SourceTreeRoot> Sources { get; private set; }

        public List<string> Binaries { get; private set; }

        public string SourceStore { get; set; }

        public string SymbolStore { get; set; }

        public string ProductName { get; set; }

        public string ProductVersion { get; set; }

        public string Comment { get; set; }

        public bool ParseCommandLine(string[] args)
        {
            var p = new FluentCommandLineParser();

            p.Setup<string>("build")
                .WithDescription("The directory to search for binaries and source code.")
                .Callback(this.SetBuildDirectory);

            p.Setup<List<string>>("src")
                .WithDescription("Directory where source files are to be collected. Files will not be collected outside this location. May be specified more than once.")
                .Callback(this.AddSourceLocationFromCliParameter);

            p.Setup<List<string>>("bin")
                .WithDescription("Directory containing PDB or EXE. This option may be specified more than once as soon as all directories have non-empty common path prefix.")
                .Callback(this.AddBinaryLocationFromCliParameter);

            p.Setup<string>("store")
                .WithDescription("Symbol and source store directory. Binaries are stored in sym subdirectory, while sources are stored in src subdirectory.")
                .Callback(this.SetStoreLocation);

            p.Setup<string>("srcstore")
                .WithDescription("Source store directory.")
                .Callback(s => this.SourceStore = s);

            p.Setup<string>("symstore")
                .WithDescription("Symbol store directory.")
                .Callback(s => this.SymbolStore = s);

            p.Setup<string>("product")
                .WithDescription("Product name")
                .Required()
                .Callback(s => this.ProductName = s);

            p.Setup<string>("version")
                .WithDescription("Product version")
                .Callback(s => this.ProductVersion = s);

            p.Setup<string>("comment")
                .WithDescription("Comment")
                .Callback(this.SetCommentFromCliParameter);

            p.Setup<List<string>>("exclude")
                .WithDescription("Exclude files matching the specified pattern. This option can be specified more than once. The pattern matching applies to both source and PDB files.")
                .Callback(this.SetExcludePatternFromCliParameter);

            p.SetupHelp("h", "help", "?")
                .UseForEmptyArgs()
                .WithHeader("Populates source and symbol store.")
                .Callback(s => Console.WriteLine(s));

            var result = p.Parse(args);
            if (result.HasErrors)
            {
                Console.WriteLine(result.ErrorText);
            }

            if (result.AdditionalOptionsFound.Any())
            {
                Console.WriteLine("error: Unknown command line options:");
                foreach (var option in result.AdditionalOptionsFound)
                {
                    Console.WriteLine("    {0}", option.Key);
                }
            }

            if (result.HelpCalled)
            {
                return false;
            }

            if (result.HasErrors)
            {
                return false;
            }

            // TODO: Go through this.Sources and find common ancestor
            // Always consider directory at the root level
            // This set:
            //  C:\sourcetree1\public\src
            //  C:\sourcetree1\
            //  C:\sourcetree2\other\src
            // Results in:
            //  C:\sourcetree1\
            //  C:\sourcetree2\other\src
            // 
            // However, this set:
            //  C:\
            //  C:\mysourcetree1\public\src
            //  C:\sourcetree1\
            //  C:\sourcetree2\other\src
            // Results in:
            //  C:\

            return true;
        }

        public bool Validate()
        {
            if (this.SourceStore == null)
            {
                Console.WriteLine("Source store directory not specified.");
                return false;
            }

            if (this.SymbolStore == null)
            {
                Console.WriteLine("Symbol store directory not specified.");
                return false;
            }

            if (this.Sources.Count == 0)
            {
                Console.WriteLine("No source directories specified.");
                return false;
            }

            if (this.Binaries.Count == 0)
            {
                Console.WriteLine("No binary directories specified.");
                return false;
            }

            return true;
        }

        private void SetBuildDirectory(string location)
        {
            this.Sources.Add(new SourceTreeRoot(location));
            this.Binaries.Add(location);
        }

        private void SetStoreLocation(string location)
        {
            this.SourceStore = Path.Combine(location, @"src");
            this.SymbolStore = Path.Combine(location, @"sym");
        }

        /// <summary>
        /// Sets transaction comment from command line interface parameter.
        /// </summary>
        /// <param name="fileOrString">
        /// Comment as a string or if prefixed with '@' (at) character,
        /// a path to a file with a comment.
        /// </param>
        private void SetCommentFromCliParameter(string fileOrString)
        {
            this.Comment = fileOrString.StartsWith("@") ?
                               File.ReadAllText(fileOrString.Substring(1)) :
                               fileOrString;
        }

        private void AddSourceLocationFromCliParameter(List<string> list)
        {
            foreach (var path in list)
            {
                var mapping = path.Split(new[] { "*" }, StringSplitOptions.RemoveEmptyEntries);
                var sourceTreeRoot = new SourceTreeRoot(mapping[0].Trim());
                if (mapping.Length >= 2 && !string.IsNullOrEmpty(mapping[1].Trim()))
                {
                    sourceTreeRoot.OriginalPath = mapping[1].Trim();
                }

                this.Sources.Add(sourceTreeRoot);
            }
        }

        private void AddBinaryLocationFromCliParameter(List<string> list)
        {
            this.Binaries.AddRange(list);
        }

        private void SetExcludePatternFromCliParameter(List<string> list)
        {
        }
    }
}