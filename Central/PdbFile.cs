namespace Central
{
    using System.Collections.Generic;

    internal class PdbFile
    {
        public PdbFile(string fileName)
        {
            this.FileName = fileName;
            this.SourceFiles = new List<SourceFileReference>();
        }

        public string FileName { get; private set; }

        public List<SourceFileReference> SourceFiles { get; private set; }

        public void AddSourceFile(SourceFileReference sourceFileReference)
        {
            this.SourceFiles.Add(sourceFileReference);
        }
    }
}