// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.Matchers
{
    public interface IComponentMatcher
    {
        public bool IsMatch(MonoBehaviour component);
    }
}
