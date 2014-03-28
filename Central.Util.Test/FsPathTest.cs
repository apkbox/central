using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Central.Util.Test
{
    using System.Security.AccessControl;

    [TestClass]
    public class FsPathTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseEmptyPath()
        {
            var path = new FsPath(string.Empty);
        }

        [TestMethod]
        public void ParseDriveOnly()
        {
            var path = new FsPath("C:");
            Assert.AreEqual("C:", path.Volume);
            Assert.AreEqual(0, path.Components.Count);
            Assert.AreEqual("C:", path.ToString());
            Assert.IsFalse(path.IsRooted);
            Assert.IsFalse(path.IsAbsolute);
            Assert.IsFalse(path.IsComplete);
        }

        [TestMethod]
        public void ParseDriveAndRoot()
        {
            var path = new FsPath(@"C:\");
            Assert.AreEqual("C:", path.Volume);
            Assert.AreEqual(0, path.Components.Count);
            Assert.AreEqual(@"C:\", path.ToString());
            Assert.IsTrue(path.IsRooted);
            Assert.IsTrue(path.IsAbsolute);
            Assert.IsFalse(path.IsComplete);
        }

        [TestMethod]
        public void ParseAbsolutePath()
        {
            var path = new FsPath(@"C:\Directory\File.txt");
            Assert.AreEqual("C:", path.Volume);
            Assert.AreEqual(2, path.Components.Count);
            CollectionAssert.AreEqual(new[] { "Directory", "File.txt" }, path.Components);
            Assert.AreEqual(@"C:\Directory\File.txt", path.ToString());
            Assert.IsTrue(path.IsRooted);
            Assert.IsTrue(path.IsAbsolute);
            Assert.IsTrue(path.IsComplete);
        }

        [TestMethod]
        public void ParseUncPath()
        {
            var path = new FsPath(@"\\SERVER\Share\Directory\File.txt");
            Assert.AreEqual(@"\\SERVER", path.Volume);
            Assert.AreEqual(3, path.Components.Count);
            CollectionAssert.AreEqual(new[] { "Share", "Directory", "File.txt" }, path.Components);
            Assert.AreEqual(@"\\SERVER\Share\Directory\File.txt", path.ToString());
            Assert.IsTrue(path.IsRooted);
            Assert.IsTrue(path.IsAbsolute);
            Assert.IsTrue(path.IsComplete);
        }

        [TestMethod]
        public void ParseRelativePath()
        {
            var path = new FsPath(@"Directory\File.txt");
            Assert.AreEqual(string.Empty, path.Volume);
            Assert.AreEqual(2, path.Components.Count);
            CollectionAssert.AreEqual(new[] { "Directory", "File.txt" }, path.Components);
            Assert.AreEqual(@"Directory\File.txt", path.ToString());
            Assert.IsFalse(path.IsRooted);
            Assert.IsFalse(path.IsAbsolute);
            Assert.IsTrue(path.IsComplete);
        }

        [TestMethod]
        public void ParseRootPath()
        {
            var path = new FsPath(@"\Directory\File.txt");
            Assert.AreEqual(string.Empty, path.Volume);
            Assert.AreEqual(2, path.Components.Count);
            CollectionAssert.AreEqual(new[] { "Directory", "File.txt" }, path.Components);
            Assert.AreEqual(@"\Directory\File.txt", path.ToString());
            Assert.IsTrue(path.IsRooted);
            Assert.IsFalse(path.IsAbsolute);
            Assert.IsTrue(path.IsComplete);
        }

        [TestMethod]
        public void ParseDriveRelativePath()
        {
            var path = new FsPath(@"C:Directory\File.txt");
            Assert.AreEqual("C:", path.Volume);
            Assert.AreEqual(2, path.Components.Count);
            CollectionAssert.AreEqual(new[] { "Directory", "File.txt" }, path.Components);
            Assert.AreEqual(@"C:Directory\File.txt", path.ToString());
            Assert.IsFalse(path.IsRooted);
            Assert.IsFalse(path.IsAbsolute);
            Assert.IsTrue(path.IsComplete);
        }

        [TestMethod]
        public void ParseFileName()
        {
            var path = new FsPath(@"File.txt");
            Assert.AreEqual(string.Empty, path.Volume);
            Assert.AreEqual(1, path.Components.Count);
            CollectionAssert.AreEqual(new[] { "File.txt" }, path.Components);
            Assert.AreEqual(@"File.txt", path.ToString());
            Assert.IsFalse(path.IsRooted);
            Assert.IsFalse(path.IsAbsolute);
            Assert.IsTrue(path.IsComplete);
        }

        [TestMethod]
        public void ParseDirectory()
        {
            var path = new FsPath(@"Directory\");
            Assert.AreEqual(string.Empty, path.Volume);
            Assert.AreEqual(1, path.Components.Count);
            CollectionAssert.AreEqual(new[] { "Directory" }, path.Components);
            Assert.AreEqual(@"Directory\", path.ToString());
            Assert.IsFalse(path.IsRooted);
            Assert.IsFalse(path.IsAbsolute);
            Assert.IsFalse(path.IsComplete);
        }

        [TestMethod]
        public void ParseRootOnly()
        {
            var path = new FsPath(@"\");
            Assert.AreEqual(string.Empty, path.Volume);
            Assert.AreEqual(0, path.Components.Count);
            Assert.AreEqual(@"\", path.ToString());
            Assert.IsTrue(path.IsRooted);
            Assert.IsFalse(path.IsAbsolute);
            Assert.IsFalse(path.IsComplete);
        }

        [TestMethod]
        public void ParseLegacyCompatibleUnc()
        {
            var path = new FsPath(@"C:\\Directory\File.txt");
            Assert.AreEqual(@"C:\", path.Volume);
            Assert.AreEqual(2, path.Components.Count);
            CollectionAssert.AreEqual(new[] { "Directory", "File.txt" }, path.Components);
            Assert.AreEqual(@"C:\\Directory\File.txt", path.ToString());
            Assert.IsTrue(path.IsRooted);
            Assert.IsTrue(path.IsAbsolute);
            Assert.IsTrue(path.IsComplete);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseInvalidUnc()
        {
            var path = new FsPath(@"\\");
        }

        [TestMethod]
        public void PathMethodsBehavior()
        {
            // Test that behavior matches System.IO.Path.
            Assert.AreEqual(
                Path.GetDirectoryName(@"C:\Directory\File.txt"),
                new FsPath(@"C:\Directory\File.txt").GetDirectoryName());
            Assert.AreEqual(
                Path.GetExtension(@"C:\Directory\File.txt"),
                new FsPath(@"C:\Directory\File.txt").GetExtension());
            Assert.AreEqual(
                Path.GetFileName(@"C:\Directory\File.txt"),
                new FsPath(@"C:\Directory\File.txt").GetFileName());
            Assert.AreEqual(
                Path.GetFileNameWithoutExtension(@"C:\Directory\File.txt"),
                new FsPath(@"C:\Directory\File.txt").GetFileNameWithoutExtension());
            Assert.AreEqual(
                Path.GetFullPath(@"C:\Directory\File.txt"),
                new FsPath(@"C:\Directory\File.txt").GetFullPath());
            Assert.AreEqual(
                Path.GetPathRoot(@"C:\Directory\File.txt"),
                new FsPath(@"C:\Directory\File.txt").GetPathRoot());
            Assert.AreEqual(
                Path.HasExtension(@"C:\Directory\File.txt"),
                new FsPath(@"C:\Directory\File.txt").HasExtension());
            Assert.AreEqual(
                Path.IsPathRooted(@"C:\Directory\File.txt"),
                new FsPath(@"C:\Directory\File.txt").IsPathRooted());
        }

        [TestMethod]
        public void CombineBehaviorShouldMatchIoPath()
        {
            Assert.AreEqual(
                Path.Combine(@"C:\Directory", @"File.txt"),
                new FsPath(@"C:\Directory").Combine(@"File.txt").ToString());
        }

        [TestMethod]
        public void IsParentOf()
        {
            Assert.IsFalse(new FsPath(@"C:\Directory").IsParentOf(@"C:\Directory"));
            Assert.IsTrue(new FsPath(@"C:\Directory").IsParentOf(@"C:\Directory\Subdirectory\File.txt"));
            Assert.IsFalse(new FsPath(@"D:\Directory").IsParentOf(@"C:\Directory\Subdirectory\File.txt"));
            Assert.IsFalse(new FsPath(@"C:\Directory\OtherDirectory").IsParentOf(@"C:\Directory\Subdirectory\File.txt"));
            Assert.IsTrue(new FsPath(@"Directory").IsParentOf(@"Directory\Subdirectory\File.txt"));
            Assert.IsTrue(new FsPath(@"Directory\Subdirectory").IsParentOf(@"Directory\Subdirectory\File.txt"));
            Assert.IsTrue(new FsPath(@"\Directory").IsParentOf(@"\Directory\Subdirectory\File.txt"));
            Assert.IsFalse(new FsPath(@"Directory").IsParentOf(@"\Directory\Subdirectory\File.txt"));
            Assert.IsFalse(new FsPath(@"\Directory").IsParentOf(@"Directory\Subdirectory\File.txt"));
            Assert.IsFalse(new FsPath(@"\Directory\..\Directory").IsParentOf(@"\Directory\Subdirectory\File.txt"));
            Assert.IsTrue(new FsPath(@"\Directory").IsParentOf(@"\Directory\..\Directory\Subdirectory\File.txt"));
            Assert.IsFalse(new FsPath(@"\Directory\Subdirectory").IsParentOf(@"\Directory\..\Directory\Subdirectory\File.txt"));
        }

        [TestMethod]
        public void IsParentOrSelf()
        {
            Assert.IsFalse(new FsPath(@"C:\Directory").IsParentOf(@"C:\Directory"));
            Assert.IsTrue(new FsPath(@"C:\Directory").IsParentOf(@"C:\Directory\Subdirectory\File.txt"));
            Assert.IsFalse(new FsPath(@"D:\Directory").IsParentOf(@"C:\Directory\Subdirectory\File.txt"));
            Assert.IsFalse(new FsPath(@"C:\Directory\OtherDirectory").IsParentOf(@"C:\Directory\Subdirectory\File.txt"));
            Assert.IsTrue(new FsPath(@"Directory").IsParentOf(@"Directory\Subdirectory\File.txt"));
            Assert.IsTrue(new FsPath(@"Directory\Subdirectory").IsParentOf(@"Directory\Subdirectory\File.txt"));
            Assert.IsTrue(new FsPath(@"\Directory").IsParentOf(@"\Directory\Subdirectory\File.txt"));
            Assert.IsFalse(new FsPath(@"Directory").IsParentOf(@"\Directory\Subdirectory\File.txt"));
            Assert.IsFalse(new FsPath(@"\Directory").IsParentOf(@"Directory\Subdirectory\File.txt"));
            Assert.IsFalse(new FsPath(@"\Directory\..\Directory").IsParentOf(@"\Directory\Subdirectory\File.txt"));
            Assert.IsTrue(new FsPath(@"\Directory").IsParentOf(@"\Directory\..\Directory\Subdirectory\File.txt"));
            Assert.IsFalse(new FsPath(@"\Directory\Subdirectory").IsParentOf(@"\Directory\..\Directory\Subdirectory\File.txt"));
        }

        [TestMethod]
        public void GetRelativePath()
        {
            Assert.AreEqual(@"Subdirectory\File.txt", new FsPath(@"C:\Directory").GetRelativePath(@"C:\Directory\Subdirectory\File.txt").ToString());
            Assert.IsNull(new FsPath(@"C:\Directory").GetRelativePath(@"D:\Directory\Subdirectory\File.txt"));
            Assert.IsNull(new FsPath(@"C:\Directory").GetRelativePath(@"D:Directory\Subdirectory\File.txt"));
            Assert.IsNull(new FsPath(@"C:\Directory").GetRelativePath(@"C:\OtherDirectory\Subdirectory\File.txt"));
        }

        [TestMethod]
        public void AppendRelativePath()
        {
            Assert.AreEqual(@"D:\OtherDirectory\Subdirectory\File.txt", 
                new FsPath(@"C:\Directory").AppendRelativePath(@"C:\Directory\Subdirectory\File.txt", @"D:\OtherDirectory").ToString());
        }

        [TestMethod]
        public void GetCommonParent()
        {
            Assert.IsNull(
                new FsPath(@"D:\Directory\Subdirectory\Level1\File.txt").GetCommonParent(
                    @"C:\Directory\Subdirectory\File.txt"));
            // TODO: Fix this
            //Assert.AreEqual(
            //    @"C:\Directory\Subdirectory",
            //    new FsPath(@"C:\Directory\Subdirectory\Level1\File.txt").GetCommonParent(
            //        @"C:\Directory\Subdirectory\File.txt").ToString());
            Assert.AreEqual(
                @"C:\Directory\Subdirectory\",
                new FsPath(@"C:\Directory\Subdirectory\").GetCommonParent(@"C:\Directory\Subdirectory\").ToString());
            Assert.AreEqual(
                @"C:\Directory\Subdirectory",
                new FsPath(@"C:\Directory\Subdirectory\").GetCommonParent(@"C:\Directory\Subdirectory").ToString());
            Assert.AreEqual(
                @"C:\Directory\Subdirectory",
                new FsPath(@"C:\Directory\Subdirectory").GetCommonParent(@"C:\Directory\Subdirectory\").ToString());
        }
    }
}
