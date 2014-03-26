using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Central.SrcStoreDb
{
    public class Transaction
    {

        public string Product { get; set; }

        public string Version { get; set; }

        public string Comment { get; set; }

        public IEnumerable<string> Files { get; set; }
    }
}
