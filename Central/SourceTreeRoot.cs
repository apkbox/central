namespace Central
{
    using System;

    public class SourceTreeRoot
    {
        public SourceTreeRoot(string path)
        {
            this.Path = path;
            this.OriginalPath = path;
        }

        public string Path { get; private set; }

        public string OriginalPath { get; set; }
    }
}