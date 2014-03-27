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
        private FsPath()
        {
            this.Volume = string.Empty;
            this.Components = new List<string>();
        }

        public FsPath(string p)
        {
            this.Volume = string.Empty;
            this.Components = new List<string>();
            Parse(p);
        }

        public string Volume { get; set; }

        // TODO: Make unmutable
        public List<string> Components { get; private set; }

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
            this.Components.Clear();
            this.Components.AddRange(parts);

            this.IsRooted = fragment.StartsWith(@"\");
            this.IsComplete = this.Components.Count > 0 && !fragment.EndsWith(@"\");
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

            path += string.Join(@"\", this.Components);
            if (this.Components.Count > 0 && !this.IsComplete)
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

        public bool HasExtension()
        {
            return Path.HasExtension(this.ToString());
        }

        public bool IsPathRooted()
        {
            return Path.IsPathRooted(this.ToString());
        }

        public FsPath Combine(string right)
        {
            return new FsPath(Path.Combine(this.ToString(), right));
        }

        // TODO: FsPath Combine(FsPath right)

        public bool IsParentOf(string child)
        {
            return IsParentOf(child, false);
        }

        public bool IsParentOrSelf(string child)
        {
            return IsParentOf(child, true);
        }

        private bool IsParentOf(string child, bool orSelf)
        {
            var childPath = new FsPath(child);

            if (this.IsRooted ^ childPath.IsRooted)
            {
                return false;
            }

            if (this.IsAbsolute ^ childPath.IsAbsolute)
            {
                return false;
            }

            if (!this.Volume.Equals(childPath.Volume, StringComparison.OrdinalIgnoreCase)) 
            {
                return false;
            }

            if (orSelf && this.Components.Count > childPath.Components.Count)
            {
                return false;
            }
            else if (this.Components.Count >= childPath.Components.Count)
            {
                return false;
            }

            return !this.Components.Where((t, i) => !t.Equals(childPath.Components[i], StringComparison.OrdinalIgnoreCase)).Any();
        }

        public FsPath GetRelativePath(string child)
        {
            if (!this.IsParentOf(child))
            {
                return null;
            }

            var childPath = new FsPath(child);
            var relativePath = new FsPath();
            for (var i = this.Components.Count; i < childPath.Components.Count; i++)
            {
                relativePath.Components.Add(childPath.Components[i]);
            }

            relativePath.IsRooted = false;
            relativePath.IsComplete = childPath.IsComplete;
            relativePath.IsAbsolute = false;

            return relativePath;
        }

        public FsPath AppendRelativePath(string child, string newParent)
        {
            var relativePath = this.GetRelativePath(child);
            if (relativePath == null)
            {
                return null;
            }

            return new FsPath(newParent).Combine(relativePath.ToString());
        }

        public FsPath GetCommonParent(string other)
        {
            var otherPath = new FsPath(other);
            return null;
        }
    }
}
