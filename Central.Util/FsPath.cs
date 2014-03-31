namespace Central.Util
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public class FsPath
    {
        private FsPath()
        {
            this.Volume = string.Empty;
        }

        public FsPath(string p)
        {
            this.Volume = string.Empty;
            Parse(p);
        }

        public string Volume { get; private set; }

        public string[] Components { get; private set; }

        public bool IsRooted { get; private set; }

        public bool IsAbsolute { get; private set; }

        // TODO: Make internal field incomplete and use !incomplete as this is
        // more convinient/clear.
        public bool IsComplete { get; private set; }

        public string GetExtension()
        {
            return Path.GetExtension(this.ToString());
        }

        public static string GetExtension(string path) {
            return Path.GetExtension(path);
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
            if (this.Components.Length > 0 && !this.IsComplete)
            {
                path += '\\';
            }

            return path;
        }

        public string GetFileName()
        {
            return Path.GetFileName(this.ToString());
        }

        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public string GetDirectoryName()
        {
            return Path.GetDirectoryName(this.ToString());
        }

        public static string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public string GetFileNameWithoutExtension()
        {
            return Path.GetFileNameWithoutExtension(this.ToString());
        }

        public static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public string GetFullPath()
        {
            return Path.GetFullPath(this.ToString());
        }

        public static string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        public string GetPathRoot()
        {
            return Path.GetPathRoot(this.ToString());
        }

        public static string GetPathRoot(string path)
        {
            return Path.GetPathRoot(path);
        }

        public bool HasExtension()
        {
            return Path.HasExtension(this.ToString());
        }

        public static bool HasExtension(string path)
        {
            return Path.HasExtension(path);
        }

        public bool IsPathRooted()
        {
            return Path.IsPathRooted(this.ToString());
        }

        public static bool IsPathRooted(string path)
        {
            return Path.IsPathRooted(path);
        }

        public FsPath Combine(FsPath right)
        {
            return new FsPath(Path.Combine(this.ToString(), right.ToString()));
        }

        public FsPath Combine(string right)
        {
            return new FsPath(Path.Combine(this.ToString(), right));
        }

        public static string Combine(string left, string right)
        {
            return Path.Combine(left, right);
        }

        public bool IsParentOf(FsPath child)
        {
            return IsParentOf(child, false);
        }

        public bool IsParentOf(string child)
        {
            return IsParentOf(new FsPath(child), false);
        }

        public static bool IsParentOf(string parent, string child)
        {
            return new FsPath(parent).IsParentOf(new FsPath(child), false);
        }

        public bool IsParentOrSelf(FsPath child)
        {
            return IsParentOf(child, true);
        }

        public bool IsParentOrSelf(string child)
        {
            return IsParentOf(new FsPath(child), true);
        }

        public static bool IsParentOrSelf(string parent, string child)
        {
            return new FsPath(parent).IsParentOf(new FsPath(child), true);
        }

        public FsPath GetRelativePath(FsPath child)
        {
            if (!this.IsParentOf(child))
            {
                return null;
            }

            var relativePath = new FsPath();
            var components = new List<string>();
            for (var i = this.Components.Length; i < child.Components.Length; i++)
            {
                components.Add(child.Components[i]);
            }

            relativePath.Components = components.ToArray();

            relativePath.IsRooted = false;
            relativePath.IsComplete = child.IsComplete;
            relativePath.IsAbsolute = false;

            return relativePath;
        }

        public FsPath GetRelativePath(string child)
        {
            return GetRelativePath(new FsPath(child));
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
            if (!this.CanBeParentOf(otherPath, true))
            {
                return null;
            }

            var commonPath = new FsPath();
            commonPath.Volume = this.Volume;
            commonPath.IsAbsolute = this.IsAbsolute;
            commonPath.IsRooted = this.IsRooted;
            if (this.Components.Length == otherPath.Components.Length)
            {
                commonPath.IsComplete = this.IsComplete || otherPath.IsComplete;
            }

            var components = new List<string>();

            for (int i = 0; i < Math.Min(this.Components.Length, otherPath.Components.Length); i++)
            {
                if (!this.Components[i].Equals(otherPath.Components[i], StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                components.Add(this.Components[i]);
            }

            commonPath.Components = components.ToArray();

            return commonPath;
        }

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
            else
            {
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

            this.Components = fragment.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            this.IsRooted = fragment.StartsWith(@"\");
            this.IsComplete = this.Components.Length > 0 && !fragment.EndsWith(@"\");
            this.IsAbsolute = this.Volume.Length > 0 && this.IsRooted;
        }

        private bool IsParentOf(FsPath child, bool orSelf)
        {
            if (!this.CanBeParentOf(child, orSelf))
            {
                return false;
            }

            if (orSelf && this.Components.Length > child.Components.Length)
            {
                return false;
            }
            else if (this.Components.Length >= child.Components.Length)
            {
                return false;
            }

            return !this.Components.Where((t, i) => !t.Equals(child.Components[i], StringComparison.OrdinalIgnoreCase)).Any();
        }

        /// <summary>
        /// Checks whether the instance can in principle be a parent of 
        /// the specified child.
        /// </summary>
        /// <param name="childPath"></param>
        /// <param name="orSelf"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method does not check actual path components.
        /// </remarks>
        private bool CanBeParentOf(FsPath child, bool orSelf)
        {
            if (this.IsRooted ^ child.IsRooted)
            {
                return false;
            }

            if (this.IsAbsolute ^ child.IsAbsolute)
            {
                return false;
            }

            if (!this.Volume.Equals(child.Volume, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

    }
}
