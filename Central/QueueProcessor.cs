namespace Central
{
    using System;
    using System.IO;
    using System.Linq;

    using Central.SrcSrv;
    using Central.SymStore;
    using Central.Util;

    using Ionic.Zip;

    class QueueProcessor
    {
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

        private static void ProcessQueuedFile(
            string queuedFile,
            string symbolStoreDirectory,
            string sourceStoreDirectory)
        {
            using (var tempScope = new TempScope())
            {
                var tempProcessingDirectory = tempScope.CreateDirectory();
                using (var zipFile = ZipFile.Read(queuedFile))
                {
                    zipFile.ExtractAll(tempProcessingDirectory);
                }

                // Collect PDB files and extract source file information.
                foreach (
                    var pdbFile in
                        Directory.EnumerateFileSystemEntries(
                            tempProcessingDirectory,
                            "*.pdb",
                            SearchOption.AllDirectories))
                {
                    Console.WriteLine(pdbFile);
                    var sourceFiles = SourceStoreHelpers.GetSourceFilesFromPdb(pdbFile);
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