// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey.Extensions;
using UnityEngine;

namespace TestHelper.Monkey.GameObjectMatchers
{
    /// <summary>
    /// <see cref="GameObject"/> matcher that matchers by hierarchy path.
    /// </summary>
    public class PathMatcher : IGameObjectMatcher
    {
        private readonly string _path;

        /// <summary>
        /// Constructor with hierarchy path.
        /// </summary>
        /// <param name="path"><see cref="GameObject"/> hierarchy path separated by `/`. Can specify wildcards of glob pattern (`?`, `*`, and `**`).</param>
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
