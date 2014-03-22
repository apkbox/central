namespace Central
{
    using System;

    public class SourceTreeRoot
    {
        public SourceTreeRoot(string path)
        {
            this.Path = path;
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public string Path { get; private set; }
    }
}