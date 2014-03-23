namespace Central
{
    using System;
    using System.Collections.Generic;
    using System.Data.Odbc;
    using System.IO;
    using System.IO.MemoryMappedFiles;
    using System.Linq;

    using Central.SrcSrv;
    using Central.SymStore;
    using Central.Util;

    internal class Engine
    {
        private readonly Parameters parameters;

        private readonly List<PdbFile> pdbFiles = new List<PdbFile>();
        private readonly List<string> binaryFiles = new List<string>();

        public Engine(Parameters parameters)
        {
            this.parameters = parameters;
        }

        public void Run()
        {
            this.CollectBinaries();
            this.ExtractSourceInformation();
            this.AnnotateAndStore();
        }

        private void CollectBinaries()
        {
            // TODO: Respect exclusion list
            foreach (var binary in this.parameters.Binaries)
            {
                if (File.Exists(binary))
                {
                    // TODO: Be a bit smarter about detemining file type
                    if (this.IsPdbFile(binary))
                    {
                        this.pdbFiles.Add(new PdbFile(Path.GetFullPath(binary)));
                    }
                    else if (this.IsBinaryFile(binary))
                    {
                        this.pdbFiles.Add(new PdbFile(Path.GetFullPath(binary)));
                    }
                }
                else
                {
                    var files = Directory.EnumerateFileSystemEntries(binary, "*", SearchOption.AllDirectories);
                    foreach (var fileName in files)
                    {
                        if (this.IsPdbFile(fileName))
                        {
                            this.pdbFiles.Add(new PdbFile(fileName));
                        }
                        else if (this.IsBinaryFile(fileName))
                        {
                            this.binaryFiles.Add(fileName);
                        }
                    }
                }
            }
        }

        private void ExtractSourceInformation()
        {
            foreach (var pdbFile in this.pdbFiles)
            {
                Console.WriteLine("==== " + pdbFile.FileName);
                var sourceFileReferences = SourceStore.GetSourceFilesFromPdb(pdbFile.FileName);

                foreach (var sourceFileReference in sourceFileReferences)
                {
                    var mappedFile = FindFileMapping(sourceFileReference);

                    if (this.IsSourceFileCollectable(mappedFile))
                    {
                        Console.WriteLine("    " + sourceFileReference + " as " + mappedFile);
                        pdbFile.AddSourceFile(mappedFile);
                    }
                }
            }
        }

        private void AnnotateAndStore()
        {
            var sourceStore = new SourceStore();
            sourceStore.SourceStoreDirectory = this.parameters.SourceStore;

            var symbolStore = new SymbolStore();
            symbolStore.SymbolStoreDirectory = this.parameters.SymbolStore;

            using (var tempScope = new TempScope())
            {
                var trans = new AddFilesTransaction(this.parameters.ProductName);
                trans.Version = this.parameters.ProductVersion;
                trans.Comment = this.parameters.Comment;

                // Source index PDB files
                foreach (var pdbFile in this.pdbFiles)
                {
                    var srcSrvIniFile = tempScope.GetUniqueName();
                    var srcSrvIniStream = File.CreateText(srcSrvIniFile);
                    srcSrvIniStream.WriteLine(@"SRCSRV: ini ------------------------------------------------");
                    srcSrvIniStream.WriteLine(@"VERSION=1");
                    srcSrvIniStream.WriteLine(@"VERCTRL=filesystem");
                    srcSrvIniStream.WriteLine(@"DATETIME={0}", DateTime.Now);
                    srcSrvIniStream.WriteLine(@"SRCSRV: variables ------------------------------------------");
                    srcSrvIniStream.WriteLine(@"SRCSRVTRG=%targ%\%var2%\%var4%\%fnfile%(%var1%)");
                    srcSrvIniStream.WriteLine("SRCSRVCMD=cmd /C echo f|xcopy \"%SOURCE_REPO%\\%var3%\" %srcsrvtrg%");
                    srcSrvIniStream.WriteLine(@"SRCSRVENV=var1=string1\bvar2=string2");
                    srcSrvIniStream.WriteLine(@"SOURCE_REPO={0}", this.parameters.SourceStore);
                    srcSrvIniStream.WriteLine(@"SRCSRV: source files ---------------------------------------");

                    foreach (var sourceFile in pdbFile.SourceFiles)
                    {
                        var relativeSourceFilePath = sourceFile.Substring(this.parameters.Sources[0].Path.Length);

                        // var1 - Original full source file path
                        // var2 - Source file path relative to the project root
                        // var3 - Relative path in the source store
                        // var4 - Source file SHA-1 hash
                        srcSrvIniStream.WriteLine(
                            @"{0}*{1}*{2}*{3}",
                            sourceFile,
                            relativeSourceFilePath,
                            SourceStore.GetRelativeStorePath(sourceFile),
                            SourceStore.GetFileHash(sourceFile));

                        sourceStore.AddFile(sourceFile);
                    }

                    srcSrvIniStream.WriteLine(@"SRCSRV: end ------------------------------------------------");
                    srcSrvIniStream.Close();

                    SourceStore.AddStreamToPdb(pdbFile.FileName, srcSrvIniFile);
                    trans.AddFile(pdbFile.FileName);
                }

                foreach (var binaryFile in this.binaryFiles)
                {
                    trans.AddFile(binaryFile);
                }

                symbolStore.Commit(trans);
            }
        }

        private string FindFileMapping(string sourceFileReference)
        {
            foreach (var sourceTreeRoot in this.parameters.Sources)
            {
                var retargetedPath = PathUtil.AppendRelativePath(
                    sourceTreeRoot.OriginalPath,
                    sourceFileReference,
                    sourceTreeRoot.Path);
                if (retargetedPath != null)
                {
                    return retargetedPath;
                }
            }

            return null;
        }

        private bool IsSourceFileCollectable(string sourceFile)
        {
            if (sourceFile == null || !File.Exists(sourceFile))
            {
                return false;
            }

            var normalizedSourcePath = Path.GetFullPath(sourceFile).ToLowerInvariant();

            foreach (var collectablePrefix in this.parameters.Sources)
            {
                var prefix = collectablePrefix.Path.ToLowerInvariant();
                var commonPrefix = PathUtil.GetCommonPath(prefix, normalizedSourcePath);
                if (commonPrefix == prefix)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsPdbFile(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return extension.Equals(".pdb", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsBinaryFile(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".dll", StringComparison.OrdinalIgnoreCase);
        }
    }
}