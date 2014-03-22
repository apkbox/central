namespace Central
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
            this.Sources = new List<string>();
            this.Binaries = new List<string>();
        }

        public List<string> Sources { get; private set; }

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
                .WithDescription("Source file or a directory containing source files.")
                .Callback(this.AddSourceLocationFromCliParameter);

            p.Setup<List<string>>("bin")
                .WithDescription("Symbol file or a directory containing binary files.")
                .Callback(this.AddBinaryLocationFromCliParameter);

            p.Setup<string>("store")
                .WithDescription("Symbol and source store location.")
                .Callback(this.SetStoreLocation);

            p.Setup<string>("srcstore")
                .WithDescription("Source store location.")
                .Callback(s => this.SourceStore = s);

            p.Setup<string>("symstore")
                .WithDescription("Symbol store location.")
                .Callback(s => this.SymbolStore = s);

            p.Setup<string>("product").WithDescription("Product name").Required().Callback(s => this.ProductName = s);

            p.Setup<string>("version").WithDescription("Product version").Callback(s => this.ProductVersion = s);

            p.Setup<string>("comment").WithDescription("Comment").Callback(this.SetCommentFromCliParameter);

            p.Setup<List<string>>("exclude")
                .WithDescription("Exclude pattern")
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

            return !result.HasErrors;
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
            this.Sources.Add(location);
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
            this.Sources.AddRange(list);
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