using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Central
{
    class Program
    {
        static void Main(string[] args)
        {
            //if (args.Length < 2)
            //{
            //    Console.WriteLine("Not enough arguments.");
            //    return;
            //}

            var symstore = new Central.SymStore.SymbolStore();
            symstore.SymbolStoreDirectory = @"D:\temp\SymStore\sym";

            var trans = new SymStore.SymbolStoreAddTransaction("Central");
            trans.Version = "1.0.0.1";
            trans.Comment = "Central-1.0.0.1-win32-build-1-trunk";
            trans.AddFile(@"D:\Projects\git\Central\Central\bin\Debug\Central.exe");
            trans.AddFile(@"D:\Projects\git\Central\Central\bin\Debug\Central.pdb");
            trans.AddFile(@"D:\Projects\git\Central\Central\bin\Debug\Central.SrcSrv.dll");
            trans.AddFile(@"D:\Projects\git\Central\Central\bin\Debug\Central.SrcSrv.pdb");
            trans.AddFile(@"D:\Projects\git\Central\Central\bin\Debug\Central.SymStore.dll");
            trans.AddFile(@"D:\Projects\git\Central\Central\bin\Debug\Central.SymStore.pdb");
            symstore.Commit(trans);
        }
    }
}
