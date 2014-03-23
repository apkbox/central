namespace Central
{
    internal class SourceFileReference
    {
        public SourceFileReference(string sourceFile, string mappedFile, string relativePath)
        {
            this.SourceFile = sourceFile;
            this.MappedFile = mappedFile;
            this.RelativePath = relativePath;
        }

        public string SourceFile { get; set; }

        public string MappedFile { get; set; }

        public string RelativePath { get; set; }
    }
}
