// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey.Extensions;
using UnityEngine;

namespace TestHelper.Monkey.GameObjectMatchers
{
    /// <summary>
    /// GameObject matcher that matchers by path.
    /// </summary>
    public class PathMatcher : IGameObjectMatcher
    {
        private readonly string _path;

        /// <summary>
        /// Constructor with GameObject path.
        /// </summary>
        /// <param name="path"><c>GameObject</c> hierarchy path separated by `/`. Can specify glob pattern</param>
        /// <seealso href="https://en.wikipedia.org/wiki/Glob_(programming)"/>
        public PathMatcher(string path)
        {
            _path = path;
        }

        /// <inheritdoc/>
        public override string ToString() => $"path={_path}";

        /// <inheritdoc/>
        public bool IsMatch(GameObject gameObject)
        {
            return gameObject.transform.MatchPath(_path);
        }
    }
}
