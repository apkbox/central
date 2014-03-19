namespace Central.SymStore
{
    using System.IO;
    using System.Threading;

    /// <summary>
    /// The symbol store.
    /// </summary>
    public class SymbolStore
    {
        public string SymbolStoreDirectory { get; set; }

        private const string LockFile = "store.lock";

        private const int MaxAttempts = 5;

        private const int DelayBetweenAttempts = 5000;

        public void Commit(SymbolStoreTransaction transaction)
        {
            Directory.CreateDirectory(this.SymbolStoreDirectory);
            var lockFilePath = Path.Combine(this.SymbolStoreDirectory, LockFile);

            for (var i = 0; i < MaxAttempts; i++)
            {
                try
                {
                    using (
                        var lockFileStream = File.Open(
                            lockFilePath,
                            FileMode.OpenOrCreate,
                            FileAccess.ReadWrite,
                            FileShare.None))
                    {
                        transaction.Commit(this.SymbolStoreDirectory);
                    }
                }
                catch (IOException)
                {
                    Thread.Sleep(DelayBetweenAttempts);
                    continue;
                }

                break;
            }
        }
    }
}
