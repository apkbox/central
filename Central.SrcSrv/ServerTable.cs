using Central.SrcStoreDb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Central.SrcSrv
{
    sealed class ServerEntry
    {
        public ServerEntry(int transactionId, string operation, string objectType,
            DateTime timestamp, string product, string version, string comment)
        {
            this.TransactionId = transactionId;
            this.Operation = operation;
            this.ObjectType = objectType;
            this.Timestamp = timestamp;
            this.Product = product;
            this.Version = version;
            this.Comment = comment;
        }

        public int TransactionId { get; set; }
        public string Operation { get; set; }
        public string ObjectType { get; set; }
        public DateTime Timestamp { get; set; }
        public string Product { get; set; }
        public string Version { get; set; }
        public string Comment { get; set; }
    };

    sealed class ServerTable
    {
        private const string kServerTableFile = "server.txt";
        private string adminDir;

        private List<ServerEntry> entries = new List<ServerEntry>();

        public ServerTable(string adminDir)
        {
            this.adminDir = adminDir;
        }

        private void ReadRecords()
        {
            var adminDir = this.adminDir;
            var serverTxtPath = Path.Combine(adminDir, kServerTableFile);
            using (var stream = new StreamReader(serverTxtPath, true))
            {
                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine();
                    var items = line.Split(new char[] { ',' });
                    if (items.Length < 8)
                    {
                        throw new InvalidDataException();
                    }

                    var transactionId = int.Parse(items[0]);
                    var operation = items[1];
                    var objectType = items[2];
                    var timestamp = DateTime.Parse(items[3]);
                    var timePart = DateTime.Parse(items[4]);
                    timestamp = timestamp.Add(timePart.TimeOfDay);
                    var product = Unquote(items[5]);
                    var version = Unquote(items[6]);
                    var comment = Unquote(items[7]);
                    
                    var entry = new ServerEntry(transactionId, operation, objectType, timestamp, product, version, comment);
                    this.entries.Add(entry);
                }
            }
        }

        private void WriteRecords()
        {
            var adminDir = this.adminDir;
            var serverTxtPath = Path.Combine(adminDir, kServerTableFile);
            using (var stream = new StreamWriter(serverTxtPath))
            {
                foreach (var record in this.entries)
                {
                    this.WriteRecord(stream, record);
                }
            }
        }

        private void WriteRecord(StreamWriter stream, ServerEntry record)
        {
            stream.WriteLine("{0:0000000000},{1},{2},{3},{4},\"{5}\",\"{6}\",\"{7}\",",
                record.TransactionId,
                record.Operation,
                record.ObjectType,
                record.Timestamp.ToShortDateString(),
                record.Timestamp.ToShortTimeString(),
                record.Product,
                record.Version,
                record.Comment);
        }

        private string Unquote(string s)
        {
            if (s.Length < 2)
                return s;

            if (s[0] == '"' && s[0] == s[s.Length - 1])
            {
                return s.Substring(1, s.Length - 2);
            }

            return s;
        }

        private void AddTransaction(int id, Transaction transaction)
        {
            this.entries.Add(new ServerEntry(id, "add", "file", DateTime.Now, transaction.Product, transaction.Version, transaction.Comment));
        }

        private void RemoveTransaction(int id)
        {
            this.entries.RemoveAll(e => e.TransactionId == id);
        }
    }
}
