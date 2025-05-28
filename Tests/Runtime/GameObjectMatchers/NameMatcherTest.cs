// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
using UnityEngine;

namespace TestHelper.Monkey.GameObjectMatchers
{
    [TestFixture]
    public class NameMatcherTest
    {
        [Test]
        public void ToString_ReturnsWithName()
        {
            var sut = new NameMatcher("Button");
            var actual = sut.ToString();
            Assert.That(actual, Is.EqualTo("name=Button"));
        }

        [Test]
        public void IsMatch_NotMatchName_ReturnsFalse()
        {
            var sut = new NameMatcher(name: "Button");
            var actual = sut.IsMatch(new GameObject("Not Button"));
            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsMatch_MatchName_ReturnsTrue()
        {
            var sut = new NameMatcher(name: "Button");
            var actual = sut.IsMatch(new GameObject("Button"));
            Assert.That(actual, Is.True);
        }
    }
}
