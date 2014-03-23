using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Central.Util.Test
{
    [TestClass]
    public class PathUtilTest
    {
        [TestMethod]
        public void AppendRelativePath()
        {
            Assert.AreEqual(@"C:\test2\child", PathUtil.AppendRelativePath(@"C:\test1", @"C:\test1\child", @"C:\test2"));
        }
    }
}
