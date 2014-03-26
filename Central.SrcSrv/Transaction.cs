namespace Central.SrcStoreDb
{
    using System.Collections.Generic;

    public class Transaction
    {
        private readonly HashSet<string> files = new HashSet<string>();

        public Transaction()
        {
        }

        public string Product { get; set; }

        public string Version { get; set; }

        public string Comment { get; set; }

        public ICollection<string> Files
        {
            get
            {
                return this.files;
            }
        }
    }
}
