// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Text.RegularExpressions;
using UnityEngine;

namespace TestHelper.Monkey.Matchers
{
    public class GameObjectNameMatcher : IComponentMatcher
    {
        private readonly Regex _namePattern;

        public GameObjectNameMatcher(string namePattern)
        {
            _namePattern = new Regex(namePattern);
        }

        public bool IsMatch(MonoBehaviour component)
        {
            return _namePattern.IsMatch(component.gameObject.name);
        }
    }
}
