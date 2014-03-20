using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Central.SymStore.Test
{
    using Central.Util;

    [TestClass]
    public class UtilTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual(@"aaaa",
                PathUtil.GetCommonPath(@"aaaa\bbbb", @"aaaa\cccc"));
        }

        [TestMethod]
        public void TestMethod2()
        {
            Assert.AreEqual(@"aaaa\bbbb",
                PathUtil.FindCommonPath(@"\", new List<string>() {
                    @"aaaa\bbbb",
                    @"aaaa\bbbb\ccccc",
                    @"aaaa\bbbb\sdfsdf",
                    @"aaaa\bbbb",
                    @"aaaa\bbbb\sds\df\dfd"
                }));
        }
    }
}
