// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.GameObjectMatchers
{
    /// <summary>
    /// <see cref="GameObject"/> matcher that matchers by name.
    /// </summary>
    public class NameMatcher : IGameObjectMatcher
    {
        private readonly string _name;

        /// <summary>
        /// Constructor with name.
        /// </summary>
        /// <param name="name"><see cref="GameObject"/> name</param>
        public NameMatcher(string name)
        {
            _name = name;
        }

        /// <inheritdoc/>
        public override string ToString() => $"name={_name}";

        /// <inheritdoc/>
        public bool IsMatch(GameObject gameObject)
        {
            return gameObject.name == _name;
        }
    }
}
