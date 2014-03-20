namespace Central.Util
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public static class PathUtil
    {
        public static string GetCommonPath(IEnumerable<string> paths)
        {
            return FindCommonPath(@"\", paths);
        }

        public static string GetCommonPath(string path1, string path2)
        {
            return FindCommonPath(@"\", new[] { path1, path2 });
        }

        public static string FindCommonPath(string separator, IEnumerable<string> paths)
        {
            var commonPath = string.Empty;

            if (paths != null && paths.Any())
            {
                var separatedPath = paths.First(str => str.Length == paths.Max(st2 => st2.Length))
                    .Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                foreach (string pathSegment in separatedPath.AsEnumerable())
                {
                    if (commonPath.Length == 0 && paths.All(str => str.StartsWith(pathSegment, StringComparison.OrdinalIgnoreCase)))
                    {
                        commonPath = pathSegment;
                    }
                    else if (paths.All(str => str.StartsWith(commonPath + separator + pathSegment, StringComparison.OrdinalIgnoreCase)))
                    {
                        commonPath += separator + pathSegment;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return commonPath;
        }
    }
}
