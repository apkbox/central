namespace Central.Util
{
    using System;
    using System.IO;
    using System.Security.Policy;

    public class TempScope : IDisposable
    {
        private string temporaryRoot;

        private bool disposed;

        private const int MaxAttempts = 5;

        ~TempScope()
        {
            this.Dispose(false);
        }

        public string CreateDirectory()
        {
            return Directory.CreateDirectory(GetTemporaryName()).FullName;
        }

        public string GetUniqueName()
        {
            return GetTemporaryName();
        }

        private void EnsureTempRoot()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("Object already disposed.");
            }

            if (this.temporaryRoot != null && Directory.Exists(this.temporaryRoot))
            {
                return;
            }

            for (var i = 0; i < MaxAttempts; i++)
            {
                var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                if (!File.Exists(tempRoot))
                {
                    this.temporaryRoot = Directory.CreateDirectory(tempRoot).FullName;
                    return;
                }
            }

            throw new IOException("Cannot create temporary directory.");
        }

        private string GetTemporaryName()
        {
            this.EnsureTempRoot();
            return Path.Combine(this.temporaryRoot, Guid.NewGuid().ToString());
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Nothing
                }

                if (this.temporaryRoot != null)
                {
                    foreach (var entryName in Directory.GetFileSystemEntries(this.temporaryRoot, "*", SearchOption.AllDirectories))
                    {
                        var attr = File.GetAttributes(entryName);
                        if ((attr & FileAttributes.ReadOnly) != 0)
                        {
                            File.SetAttributes(entryName, attr & ~FileAttributes.ReadOnly);
                        }
                    }

                    try
                    {
                        Directory.Delete(this.temporaryRoot, true);
                    }
                    catch (IOException)
                    {
                        // Nothing we can do if directory is locked. Just silently ignore.
                    }
                }

                this.temporaryRoot = null;
                this.disposed = true;
            }
        }
    }
}