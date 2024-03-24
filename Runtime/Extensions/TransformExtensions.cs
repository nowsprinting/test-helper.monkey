// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TestHelper.Monkey.Extensions
{
    public static class TransformExtensions
    {
        /// <summary>
        /// Returns hierarchy path of <paramref name="transform"/>.
        /// </summary>
        /// <param name="transform">target</param>
        /// <returns>Path of object hierarchy. Separator is `/`</returns>
        public static string GetPath(this Transform transform)
        {
            var path = new StringBuilder();
            while (transform != null)
            {
                path.Insert(0, transform.name);
                path.Insert(0, "/");
                transform = transform.parent;
            }

            return path.ToString();
        }

        /// <summary>
        /// Judges whether a hierarchy path matches a glob pattern.
        /// </summary>
        /// <param name="transform">target</param>
        /// <param name="glob">Hierarchy path can specify glob pattern</param>
        /// <returns>True if match</returns>
        /// <seealso href="https://en.wikipedia.org/wiki/Glob_(programming)"/>
        public static bool MatchPath(this Transform transform, string glob)
        {
            if (!ValidateGlobPattern(glob))
            {
                throw new ArgumentException($"Wildcards cannot be used in the most right section of path: {glob}");
            }

            var regex = ConvertRegexFromGlob(glob);
            return regex.IsMatch(transform.GetPath());
        }

        private static bool ValidateGlobPattern(string glob)
        {
            var right = glob.Split('/').Last();
            return right.IndexOfAny(new[] { '*', '?' }) < 0;
        }

        private static Regex ConvertRegexFromGlob(string glob)
        {
            var regex = new StringBuilder();
            foreach (var c in glob)
            {
                switch (c)
                {
                    case '*':
                        regex.Append("[^/]*");
                        break;
                    case '?':
                        regex.Append("[^/]");
                        break;
                    case '.':
                        regex.Append("\\.");
                        break;
                    case '\\':
                        regex.Append("\\\\");
                        break;
                    default:
                        regex.Append(c);
                        break;
                }
            }

            regex.Replace("[^/]*[^/]*", ".*"); // globstar (**)
            regex.Insert(0, "^");
            regex.Append("$");
            return new Regex(regex.ToString());
        }
    }
}
