using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Central.Util
{
    public class FsPath
    {
        public FsPath ()
        {
            this.Volume = string.Empty;
            this.Parts = new List<string>();
        }

        public FsPath(string p)
        {
            this.Volume = string.Empty;
            this.Parts = new List<string>();
            Parse(p);
        }

        public string Volume { get; set; }

        public List<string> Parts { get; private set; }

        public bool IsRooted { get; set; }

        public bool IsAbsolute { get; set; }

        public bool IsComplete { get; set; }

        private void Parse(string path)
        {
            if (path.Length == 0)
            {
                throw new ArgumentException("Path is not in legal form.");
            }

            var fragment = path;

            if (fragment.StartsWith(@"\\"))
            {
                // UNC path.
                // TODO: Handle \\?\
                // TODO: Handle //

                var uncPattern = new Regex(@"^\\\\[\w\d!@#$%^&()_'{}~-]+");
                var match = uncPattern.Match(fragment);
                if (!match.Success)
                {
                    throw new ArgumentException();
                }

                this.Volume = match.Value;
                fragment = fragment.Substring(match.Length);
            }
            else {
                var volumePattern = new Regex(@"^[a-zA-Z]:(\\\\)?");
                var match = volumePattern.Match(fragment);
                if (match.Success)
                {
                    this.Volume = match.Value;
                    fragment = fragment.Substring(match.Length);
                    if (this.Volume.EndsWith(@"\\"))
                    {
                        this.Volume = this.Volume.Substring(0, this.Volume.Length - 1);
                        fragment = fragment.Insert(0, @"\");
                    }
                }
            }

            fragment = fragment.Replace('/', '\\');

            var parts = fragment.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            this.Parts.AddRange(parts);

            this.IsRooted = fragment.StartsWith(@"\");
            this.IsComplete = this.Parts.Count > 0 && !fragment.EndsWith(@"\");
            this.IsAbsolute = this.Volume.Length > 0 && this.IsRooted;
        }

        public string GetExtension()
        {
            return Path.GetExtension(this.ToString());
        }

        public override string ToString()
        {
            string path = "";
            if (!string.IsNullOrEmpty(this.Volume))
            {
                path += this.Volume;
            }

            if (this.IsRooted)
            {
                path += '\\';
            }

            path += string.Join(@"\", this.Parts);
            if (this.Parts.Count > 0 && !this.IsComplete)
            {
                path += '\\';
            }

            return path;
        }

        public object GetFileName()
        {
            return Path.GetFileName(this.ToString());
        }

        public string GetDirectoryName()
        {
            return Path.GetDirectoryName(this.ToString());
        }

        public string GetFileNameWithoutExtension()
        {
            return Path.GetFileNameWithoutExtension(this.ToString());
        }

        public string GetFullPath()
        {
            return Path.GetFullPath(this.ToString());
        }

        public string GetPathRoot()
        {
            return Path.GetPathRoot(this.ToString());
        }

        public object HasExtension()
        {
            return Path.HasExtension(this.ToString());
        }

        public object IsPathRooted()
        {
            return Path.IsPathRooted(this.ToString());
        }
    }
}
