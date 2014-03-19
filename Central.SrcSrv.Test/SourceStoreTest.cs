namespace Central.SrcSrv.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;

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
            this.store.SourceStoreDirectory = this.CreateUniqueTemporaryDirectory().FullName;
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
                                                       @"hello_world.c\eb29fc180f89bb0a24b40648bfb984c2c6d7af5f\hello_world.c"
                                                   },
                                                   {
                                                       @"testInput\hello_world.cpp",
                                                       @"hello_world.cpp\eb29fc180f89bb0a24b40648bfb984c2c6d7af5f\hello_world.cpp"
                                                   }
                                               };

            foreach (var pair in sourceFileToStoreFileMap)
            {
                Assert.AreEqual(
                    this.store.GetStorePath(pair.Key),
                    Path.Combine(this.store.SourceStoreDirectory, pair.Value),
                    true);
            }
        }
    }
}
