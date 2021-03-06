﻿namespace Central
{
    using System;
    using System.Collections.Generic;
    using System.Data.Odbc;
    using System.IO;
    using System.IO.MemoryMappedFiles;
    using System.Linq;

    using Central.SrcSrv;
    using Central.SrcStoreDb;
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
            Console.WriteLine("Collecting binaries from:");
            foreach (var n in parameters.Binaries)
            {
                Console.WriteLine(n);
            }

            Console.WriteLine("Collecting sources from: ");
            foreach (var n in parameters.Sources)
            {
                Console.WriteLine(n.Path);
            }

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
                var sourceFileReferences = SourceStoreHelpers.GetSourceFilesFromPdb(pdbFile.FileName);

                foreach (var sourceFileReference in sourceFileReferences)
                {
                    var reference = FindFileMapping(sourceFileReference);

                    if (reference != null && this.IsSourceFileCollectable(reference.MappedFile))
                    {
                        Console.WriteLine("    " + sourceFileReference + " as " + reference.MappedFile);
                        pdbFile.AddSourceFile(reference);
                    }
                    else {
                        Console.WriteLine("    rejected: " + sourceFileReference + " as " + 
                            (reference != null ? reference.MappedFile : string.Empty));
                    }
                }
            }
        }

        private void AnnotateAndStore()
        {
            var sourceStore = new SourceStore();
            sourceStore.StoreDirectory = this.parameters.SourceStore;

            var symbolStore = new SymbolStore();
            symbolStore.SymbolStoreDirectory = this.parameters.SymbolStore;

            using (var tempScope = new TempScope())
            {
                var symStoreTransaction = new AddFilesTransaction(this.parameters.ProductName);
                symStoreTransaction.Version = this.parameters.ProductVersion;
                symStoreTransaction.Comment = this.parameters.Comment;

                var srcStoreTransaction = new Transaction();
                srcStoreTransaction.Product = this.parameters.ProductName;
                srcStoreTransaction.Version = this.parameters.ProductVersion;
                srcStoreTransaction.Comment = this.parameters.Comment;

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
                        var relativeSourceFilePath = sourceFile.RelativePath;

                        // var1 - Original full source file path
                        // var2 - Source file path relative to the project root
                        // var3 - Relative path in the source store
                        // var4 - Source file SHA-1 hash
                        srcSrvIniStream.WriteLine(
                            @"{0}*{1}*{2}*{3}",
                            sourceFile.SourceFile,
                            relativeSourceFilePath,
                            SourceStore.GetRelativeStorePath(sourceFile.SourceFile),
                            SourceStore.GetFileHash(sourceFile.SourceFile));

                        srcStoreTransaction.Files.Add(sourceFile.MappedFile);
                    }

                    srcSrvIniStream.WriteLine(@"SRCSRV: end ------------------------------------------------");
                    srcSrvIniStream.Close();

                    SourceStoreHelpers.AddStreamToPdb(pdbFile.FileName, srcSrvIniFile);
                    symStoreTransaction.AddFile(pdbFile.FileName);
                }

                foreach (var binaryFile in this.binaryFiles)
                {
                    symStoreTransaction.AddFile(binaryFile);
                }

                symbolStore.Commit(symStoreTransaction);
                sourceStore.Commit(srcStoreTransaction);
            }
        }

        private SourceFileReference FindFileMapping(string sourceFileReference)
        {
            foreach (var sourceTreeRoot in this.parameters.Sources)
            {
                var originalPath = new FsPath(sourceTreeRoot.OriginalPath);
                var relativePath = originalPath.GetRelativePath(sourceFileReference);
                var retargetedPath = originalPath.AppendRelativePath(sourceFileReference, sourceTreeRoot.Path);
                if (retargetedPath != null)
                {
                    return new SourceFileReference(sourceFileReference, retargetedPath.ToString(), relativePath.ToString());
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
                if (FsPath.IsParentOrSelf(prefix, normalizedSourcePath))
                // var commonPrefix = PathUtil.GetCommonPath(prefix, normalizedSourcePath);
                //if (commonPrefix == prefix)
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