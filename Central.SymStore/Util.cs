namespace Central.SymStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Util
    {
        public static string GetCommonPath(IEnumerable<string> paths)
        {
            return FindCommonPath(@"\", paths);
        }

        public static string GetCommonPath(string path1, string path2)
        {
            return FindCommonPath(@"\", new string[] { path1, path2 });
        }

        public static string FindCommonPath(string Separator, IEnumerable<string> Paths)
        {
            string CommonPath = String.Empty;
            List<string> SeparatedPath = Paths
                .First(str => str.Length == Paths.Max(st2 => st2.Length))
                .Split(new string[] { Separator }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            foreach (string PathSegment in SeparatedPath.AsEnumerable())
            {
                if (CommonPath.Length == 0 && Paths.All(str => str.StartsWith(PathSegment)))
                {
                    CommonPath = PathSegment;
                }
                else if (Paths.All(str => str.StartsWith(CommonPath + Separator + PathSegment)))
                {
                    CommonPath += Separator + PathSegment;
                }
                else
                {
                    break;
                }
            }

            return CommonPath;
        }
    }
}
