﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Central.SrcStoreDb
{
    public class Database
    {
        private string storeDirectory;

        public Database(string storeDirectory)
        {
            this.storeDirectory = storeDirectory;
        }

        public void Commit(Transaction transaction)
        {
            var id = NewTransactionId();
            WriteServerRecord(id, transaction);
            WriteHistoryRecord(id, transaction);

            var transactionFileName = Path.Combine(this.GetAdminDir(), string.Format("{0:0000000000}", id));
            // Write transaction file 00000000X
            using (var stream = new StreamWriter(transactionFileName))
            {
                foreach (var file in transaction.Files)
                {
                    AddFile(id, stream, file);
                }
            }
        }


        public static string GetRelativeStorePath(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentException("File path cannot be empty", "filePath");
            }

            string fileHash = GetFileHash(filePath);
            string fileName = Path.GetFileName(filePath);

            return Path.Combine(Path.Combine(fileName, fileHash), fileName);
        }

        public static string GetFileHash(string filePath)
        {
            string fileHash;
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sha1 = new SHA1Managed())
                {
                    fileHash = BitConverter.ToString(sha1.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
                }
            }
            return fileHash;
        }

        public string GetStorePath(string filePath)
        {
            if (this.storeDirectory == null)
            {
                throw new InvalidOperationException("storeDirectory not specified.");
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
                Path.Combine(this.storeDirectory, Path.GetFileName(filePath)),
                fileHash);

            return Path.Combine(fileStoreDirectory, Path.GetFileName(filePath));
        }

        public string AddFile(int transactionId, TextWriter transactionStream, string filePath)
        {
            var storeFilePath = this.GetStorePath(filePath);
            var storeFileDirectory = Path.GetDirectoryName(storeFilePath);

            Directory.CreateDirectory(storeFileDirectory);
            if (!File.Exists(storeFilePath))
            {
                File.Copy(filePath, storeFilePath, true);
            }

            string fileHash = GetFileHash(filePath);
            string fileName = Path.GetFileName(filePath);
            transactionStream.WriteLine("\"{0}\",\"{1}\"", fileName + "\\" + fileHash, filePath);

            var refsFile = Path.Combine(storeFileDirectory, @"refs.ptr");

            using (var stream = new StreamWriter(refsFile, true))
            {
                // TODO: Quote
                stream.WriteLine("{0:0000000000},{1},\"{2}\",",
                    transactionId, "file", filePath);
            }

            return storeFilePath;
        }

        private void WriteServerRecord(int id, Transaction transaction)
        {
            WriteTransactionRecord(id, transaction, @"server.txt");
        }

        private void WriteHistoryRecord(int id, Transaction transaction)
        {
            WriteTransactionRecord(id, transaction, @"history.txt");
        }

        private void WriteTransactionRecord(int id, Transaction transaction, string file)
        {
            var timestamp = DateTime.Now;
            var adminDir = GetAdminDir();
            var serverTxtPath = Path.Combine(adminDir, file);
            using (var stream = new StreamWriter(serverTxtPath, true))
            {
                // TODO: Quote
                stream.WriteLine("{0:0000000000},{1},{2},{3},{4},\"{5}\",\"{6}\",\"{7}\",",
                    id, "add", "file", timestamp.ToShortDateString(),
                    timestamp.ToShortTimeString(), transaction.Product,
                    transaction.Version, transaction.Comment);
            }
        }

        private int NewTransactionId()
        {
            var adminDir = GetAdminDir();
            Directory.CreateDirectory(adminDir);
            var lastidTxtPath = Path.Combine(adminDir, @"lastid.txt");
            int id = 0;

            using (var stream = File.Open(lastidTxtPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                using (var reader = new StreamReader(stream))
                {
                    var idline = reader.ReadLine();
                    if (!int.TryParse(idline, out id))
                        id = 0;
                }

                stream.Seek(0, SeekOrigin.Begin);
                id++;
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write("{0:0000000000}", id);
                }
            }

            return id;
        }

        private string GetAdminDir()
        {
            var adminDir = Path.Combine(this.storeDirectory, "000Admin");
            Directory.CreateDirectory(adminDir);
            return adminDir;
        }
    }
}
