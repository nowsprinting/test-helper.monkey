// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TestHelper.Monkey.GameObjectMatchers
{
    [TestFixture]
    public class PathMatcherTest
    {
        [Test]
        public void ToString_ReturnsWithPath()
        {
            var sut = new PathMatcher("/Path/To/Button");
            var actual = sut.ToString();
            Assert.That(actual, Is.EqualTo("path=/Path/To/Button"));
        }


        [Test]
        public void IsMatch_NotMatchPath_ReturnsFalse()
        {
            var sut = new PathMatcher("/Path/To/Button");
            var actual = sut.IsMatch(CreateGameObject("/Path/To/Not/Button"));
            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsMatch_MatchPath_ReturnsTrue()
        {
            var sut = new PathMatcher("/Path/To/Button");
            var actual = sut.IsMatch(CreateGameObject("/Path/To/Button"));
            Assert.That(actual, Is.True);
        }

        // TODO: using glob

        private static GameObject CreateGameObject(string path)
        {
            using var enumerator = path.Split("/").Reverse().GetEnumerator();
            enumerator.MoveNext();
            var gameObject = new GameObject(enumerator.Current!);
            var lastGameObject = gameObject;
            while (enumerator.MoveNext())
            {
                var node = enumerator.Current!;
                if (string.IsNullOrEmpty(node))
                {
                    continue;
                }

                var parent = new GameObject(node);
                lastGameObject.transform.SetParent(parent.transform);
                lastGameObject = parent;
            }

            return gameObject;
        }
    }
}
