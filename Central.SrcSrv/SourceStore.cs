namespace Central.SrcSrv
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    public class SourceStore
    {
        public string SourceStoreDirectory { get; set; }

        public SourceStore()
        {
        }

        public string GetStorePath(string filePath)
        {
            if (this.SourceStoreDirectory == null)
            {
                throw new InvalidOperationException("SourceStoreDirectory not specified.");
            }

            if (filePath == null)
            {
                throw new ArgumentException("File path cannot be empty", "filePath");
            }

            string fileHash;

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sha1 = new SHA1Managed())
                {
                    fileHash = BitConverter.ToString(sha1.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
                }
            }

            string fileStoreDirectory = Path.Combine(
                Path.Combine(this.SourceStoreDirectory, Path.GetFileName(filePath)),
                fileHash);

            return Path.Combine(fileStoreDirectory, Path.GetFileName(filePath));
        }

        public string AddFile(string filePath)
        {
            var storeFilePath = this.GetStorePath(filePath);
            var storeFileDirectory = Path.GetDirectoryName(storeFilePath);

            Directory.CreateDirectory(storeFileDirectory);

            if (!File.Exists(storeFilePath))
            {
                File.Copy(filePath, storeFilePath, true);
            }

            return storeFilePath;
        }
    }
}
