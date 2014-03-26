namespace Central.SrcSrv.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Central.SrcStoreDb;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SourceStoreTest
    {
        private SourceStore store;

        private string GetUniqueTemporaryDirectory()
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }

        private DirectoryInfo CreateUniqueTemporaryDirectory()
        {
            return Directory.CreateDirectory(this.GetUniqueTemporaryDirectory());
        }

        [TestInitialize]
        public void Initialize()
        {
            this.store = new SourceStore();
            this.store.StoreDirectory = this.CreateUniqueTemporaryDirectory().FullName;
        }

        [TestMethod]
        public void PathGeneration()
        {
            var sourceFileToStoreFileMap = new Dictionary<string, string>
                                               {
                                                   {
                                                       @"testInput\dir1\hello_world.c",
                                                       @"hello_world.c\e0fde9ac19c68bd90f0e53e6796c944669417378\hello_world.c"
                                                   },
                                                   {
                                                       @"testInput\dir2\hello_world.c",
                                                       @"hello_world.c\e0fde9ac19c68bd90f0e53e6796c944669417378\hello_world.c"
                                                   },
                                                   {
                                                       @"testInput\dir1\hello_world.cpp",
                                                       @"hello_world.cpp\e0fde9ac19c68bd90f0e53e6796c944669417378\hello_world.cpp"
                                                   },
                                                   {
                                                       @"testInput\dir2\hello_world.cpp",
                                                       @"hello_world.cpp\e0fde9ac19c68bd90f0e53e6796c944669417378\hello_world.cpp"
                                                   },
                                                   {
                                                       @"testInput\hello_world.c",
                                                       @"hello_world.c\73f6b02845272f2da5edd99732e25362a6f1d99f\hello_world.c"
                                                   },
                                                   {
                                                       @"testInput\hello_world.cpp",
                                                       @"hello_world.cpp\73f6b02845272f2da5edd99732e25362a6f1d99f\hello_world.cpp"
                                                   }
                                               };

            foreach (var pair in sourceFileToStoreFileMap)
            {
                Assert.AreEqual(
                    Path.Combine(this.store.StoreDirectory, pair.Value),
                    this.store.GetStorePath(pair.Key),
                    true);
            }
        }
    }
}
