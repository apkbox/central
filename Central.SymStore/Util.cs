namespace Central.SymStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class Util
    {
        public static string GetCommonPath(IEnumerable<string> paths)
        {
            return string.Join(
                "/",
                paths.Select(s => s.Split('/').AsEnumerable())
                    .Transpose()
                    .TakeWhile(s => s.All(d => d == s.First()))
                    .Select(s => s.First()));
        }

        public static string GetCommonPath(string path1, string path2)
        {
            var xs = new[] { path1, path2 };

            return string.Join(
                "/",
                xs.Select(s => s.Split('/').AsEnumerable())
                    .Transpose()
                    .TakeWhile(s => s.All(d => d == s.First()))
                    .Select(s => s.First()));
        }

        public static IEnumerable<IEnumerable<T>> Transpose<T>(this IEnumerable<IEnumerable<T>> source)
        {
            var enumerators = source.Select(e => e.GetEnumerator()).ToArray();
            try
            {
                while (enumerators.All(e => e.MoveNext()))
                {
                    yield return enumerators.Select(e => e.Current).ToArray();
                }
            }
            finally
            {
                Array.ForEach(enumerators, e => e.Dispose());
            }
        }
    }
}
