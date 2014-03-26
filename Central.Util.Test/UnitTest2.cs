using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Central.Util.Test
{
    [TestClass]
    public class FsPathTest
    {
        [TestMethod]
        public void ParseEmptyPath()
        {
            var path = new FsPath(string.Empty);
            Assert.AreEqual(string.Empty, path.Volume);
            Assert.AreEqual(0, path.Parts.Count);
            Assert.AreEqual(string.Empty, path.ToString());
            Assert.IsFalse(path.IsRooted);
            Assert.IsFalse(path.IsAbsolute);
            Assert.IsFalse(path.IsComplete);
        }

        [TestMethod]
        public void ParseDriveOnly()
        {
            var path = new FsPath("C:");
            Assert.AreEqual("C:", path.Volume);
            Assert.AreEqual(0, path.Parts.Count);
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
            Assert.AreEqual(0, path.Parts.Count);
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
            Assert.AreEqual(2, path.Parts.Count);
            CollectionAssert.AreEqual(new[] { "Directory", "File.txt" }, path.Parts);
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
            Assert.AreEqual(3, path.Parts.Count);
            CollectionAssert.AreEqual(new[] { "Share", "Directory", "File.txt" }, path.Parts);
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
            Assert.AreEqual(2, path.Parts.Count);
            CollectionAssert.AreEqual(new[] { "Directory", "File.txt" }, path.Parts);
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
            Assert.AreEqual(2, path.Parts.Count);
            CollectionAssert.AreEqual(new[] { "Directory", "File.txt" }, path.Parts);
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
            Assert.AreEqual(2, path.Parts.Count);
            CollectionAssert.AreEqual(new[] { "Directory", "File.txt" }, path.Parts);
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
            Assert.AreEqual(1, path.Parts.Count);
            CollectionAssert.AreEqual(new[] { "File.txt" }, path.Parts);
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
            Assert.AreEqual(1, path.Parts.Count);
            CollectionAssert.AreEqual(new[] { "Directory" }, path.Parts);
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
            Assert.AreEqual(0, path.Parts.Count);
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
            Assert.AreEqual(2, path.Parts.Count);
            CollectionAssert.AreEqual(new[] { "Directory", "File.txt" }, path.Parts);
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
        public void PathBehavior()
        {
            Assert.AreEqual(Path.GetExtension(@"C:\File.txt"), new FsPath(@"C:\File.txt").GetExtension());
            Assert.AreEqual(Path.GetFileName(@"C:\File.txt"), new FsPath(@"C:\File.txt").GetFileName());
            // TODO: Test that behavior matches System.IO.Path methods.
        }
    }
}
