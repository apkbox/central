namespace Central
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Central.SrcSrv;
    using Central.SymStore;
    using Central.Util;

    using Fclp;

    using Ionic.Zip;

    class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <remarks>
        /// <para>
        ///  -build directory         Source and symbol directory root.
        ///  -sym file                PDB or EXE file. This option may be specified more than once.
        ///                           This option may be specified more than once as soon as
        ///                           all directories have non-empty common path prefix.
        ///  -sym dir                 Directory containing PDB or EXE.
        ///                           This option may be specified more than once as soon as
        ///                           all directories have non-empty common path prefix.
        ///  -sym @file_list          List of files.
        ///  -src sources_dir         Directory that designates source files to be collected.
        ///                           Utility will not collect referenced files outside this location.
        ///                           May be specified more than once.
        ///  -store store_dir         Symbol and source store directory.
        ///                           Binaries are stored in src subdirectory, while
        ///                           sources are stored in sym subdirectory.
        ///  -symstore sym_store_dir  Symbol store directory.
        ///  -srcstore src_store_dir  Source code store directory.
        ///  -product                 Product name
        ///  -version                 Version
        ///  -comment                 Comment
        ///  -exclude pattern         Exclude files matching the specified pattern.
        ///                           This option can be specified more than once.
        ///                           The pattern matching applies to both source and
        ///                           PDB files.
        ///  -exclude @pattern_file
        /// </para>
        /// </remarks>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            var parameters = new Parameters();
            if (!parameters.ParseCommandLine(args))
            {
                return;
            }

            if (!parameters.Validate())
            {
                return;
            }

            var engine = new Engine(parameters);
            engine.Run();

            return;
            string buildDirectory = args[0];
            string symbolStoreDirectory = args[1];
            string sourceStoreDirectory = args[2];

            var pdbFiles = new List<PdbFile>();

            IEnumerable<string> symbolFiles = Directory.EnumerateFileSystemEntries(buildDirectory, "*.pdb", SearchOption.AllDirectories).Union(
                Directory.EnumerateFileSystemEntries(buildDirectory, "*.exe", SearchOption.AllDirectories));

            // Collect PDB files and extract source file information.
            foreach (var pdbFilePath in symbolFiles)
            {
                Console.WriteLine("=============== " + pdbFilePath);

                var pdbFile = new PdbFile(pdbFilePath);
                pdbFiles.Add(pdbFile);
                
                var sourceFiles = SourceStore.GetSourceFilesFromPdb(pdbFilePath);
                foreach (var sourceFilePath in sourceFiles)
                {
                    var commonPath = PathUtil.GetCommonPath(buildDirectory, sourceFilePath);
                    var isCommon = commonPath.StartsWith(buildDirectory, StringComparison.OrdinalIgnoreCase);
                    var exists = File.Exists(sourceFilePath);
                    var notPch = !sourceFilePath.EndsWith(".pch", StringComparison.OrdinalIgnoreCase);
                    if (isCommon && exists && notPch)
                    {
                        Console.WriteLine("    " + sourceFilePath);
                        pdbFile.AddSourceFile(sourceFilePath);
                    }
                    else
                    {
                        Console.WriteLine("    skipped: " + sourceFilePath);
                        Console.WriteLine("       build: " + buildDirectory);
                        Console.WriteLine("       common: " + commonPath);
                        Console.WriteLine("         {0} {1} {2}: ", isCommon, exists, notPch);
                    }
                }
            }

            var sourceStore = new SourceStore();
            sourceStore.SourceStoreDirectory = sourceStoreDirectory;

            var symbolStore = new SymbolStore();
            symbolStore.SymbolStoreDirectory = symbolStoreDirectory;

            using (var tempScope = new TempScope())
            {
                var trans = new AddFilesTransaction("Test product");

                // Source index PDB files
                foreach (var pdbFile in pdbFiles)
                {
                    var srcSrvIniFile = tempScope.GetUniqueName();
                    var srcsrvStream = File.CreateText(srcSrvIniFile);
                    srcsrvStream.WriteLine(@"SRCSRV: ini ------------------------------------------------");
                    srcsrvStream.WriteLine(@"VERSION=1");
                    srcsrvStream.WriteLine(@"VERCTRL=fileshare");
                    srcsrvStream.WriteLine(@"DATETIME={0}", DateTime.Now);
                    srcsrvStream.WriteLine(@"SRCSRV: variables ------------------------------------------");
                    srcsrvStream.WriteLine(@"SRCSRVTRG=%targ%\%var2%\%var4%\%fnfile%(%var1%)");
                    srcsrvStream.WriteLine("SRCSRVCMD=cmd /C echo f|xcopy \"%SOURCE_REPO%\\%var3%\" %srcsrvtrg%");
                    srcsrvStream.WriteLine(@"SRCSRVENV=var1=string1\bvar2=string2");
                    srcsrvStream.WriteLine(@"SOURCE_REPO={0}", sourceStoreDirectory);
                    srcsrvStream.WriteLine(@"SRCSRV: source files ---------------------------------------");

                    foreach (var sourceFile in pdbFile.SourceFiles)
                    {
                        var relativeSourceFilePath = sourceFile.Substring(buildDirectory.Length);

                        // var1 - Original full source file path
                        // var2 - Source file path relative to the project root
                        // var3 - Relative path in the source store
                        // var4 - Source file SHA-1 hash
                        srcsrvStream.WriteLine(
                            @"{0}*{1}*{2}*{3}",
                            sourceFile,
                            relativeSourceFilePath,
                            SourceStore.GetRelativeStorePath(sourceFile),
                            SourceStore.GetFileHash(sourceFile));

                        sourceStore.AddFile(sourceFile);
                    }

                    srcsrvStream.WriteLine(@"SRCSRV: end ------------------------------------------------");
                    srcsrvStream.Close();

                    SourceStore.AddStreamToPdb(pdbFile.FileName, srcSrvIniFile);
                    trans.AddFile(pdbFile.FileName);
                }

                symbolStore.Commit(trans);
            }
        }

        private void ProcessQueue(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Not enough arguments.");
                return;
            }

            string queueDirectory = args[0];
            string symbolStoreDirectory = args[1];
            string sourceStoreDirectory = args[2];

            var queuedFiles =
                Directory.EnumerateFiles(queueDirectory)
                    .Where(s => s.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

            foreach (var queuedFile in queuedFiles)
            {
                ProcessQueuedFile(queuedFile, symbolStoreDirectory, sourceStoreDirectory);
            }
        }

        private static void ProcessQueuedFile(string queuedFile, string symbolStoreDirectory, string sourceStoreDirectory)
        {
            using (var tempScope = new TempScope())
            {
                var tempProcessingDirectory = tempScope.CreateDirectory();
                using (var zipFile = ZipFile.Read(queuedFile))
                {
                    zipFile.ExtractAll(tempProcessingDirectory);
                }

                // Collect PDB files and extract source file information.
                foreach (var pdbFile in Directory.EnumerateFileSystemEntries(tempProcessingDirectory, "*.pdb", SearchOption.AllDirectories))
                {
                    Console.WriteLine(pdbFile);
                    var sourceFiles = SourceStore.GetSourceFilesFromPdb(pdbFile);
                    foreach (var sourceFile in sourceFiles)
                    {
                        Console.WriteLine(sourceFile);
                    }
                }

                var symStore = new SymbolStore();
                symStore.SymbolStoreDirectory = symbolStoreDirectory;

                // TODO: Extract product name from zip manifest, external file or use defaults.
                var trans = new AddDirectoryTransaction("Central");
                trans.Version = "1.0.0.1";
                trans.Comment = "Central-1.0.0.1-win32-build-1-trunk";

                trans.AddDirectory(tempProcessingDirectory);
                symStore.Commit(trans);
            }
        }
    }
}
