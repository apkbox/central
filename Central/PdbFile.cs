namespace Central
{
    using System.Collections.Generic;

    internal class PdbFile
    {
        public PdbFile(string fileName)
        {
            this.FileName = fileName;
            this.SourceFiles = new List<string>();
        }

        public string FileName { get; private set; }

        public List<string> SourceFiles { get; private set; }

        public void AddSourceFile(string sourceFileName)
        {
            this.SourceFiles.Add(sourceFileName);
        }
    }
}