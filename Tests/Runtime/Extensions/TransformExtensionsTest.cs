// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using NUnit.Framework;
using UnityEngine;

namespace TestHelper.Monkey.Extensions
{
    [TestFixture]
    public class TransformExtensionsTest
    {
        private static GameObject CreateThreeGenerationObjects()
        {
            var parent = new GameObject("Parent");
            var child = new GameObject("Child") { transform = { parent = parent.transform } };
            var grandChild = new GameObject("Grandchild") { transform = { parent = child.transform } };
            return grandChild;
        }

        [Test]
        public void GetPath_NestedObject_GotPathStringSeparatedBySlashes()
        {
            var grandchild = CreateThreeGenerationObjects();
            var actual = grandchild.transform.GetPath();
            Assert.That(actual, Is.EqualTo("/Parent/Child/Grandchild"));
        }

        [Test]
        public void GetPath_RootObject_GotStringStartingWithSlash()
        {
            var solo = new GameObject("Solo");
            var actual = solo.transform.GetPath();
            Assert.That(actual, Is.EqualTo("/Solo"));
        }

        [TestCase("/Parent/*/Grandchild")]
        [TestCase("/Parent/**/Grandchild")]
        [TestCase("/Parent/Child*/Grandchild")]
        [TestCase("/Parent/Ch*/Grandchild")]
        [TestCase("/Parent/C*d/Grandchild")]
        [TestCase("/Parent/Chi?d/Grandchild")]
        [TestCase("/*/*/Grandchild")]
        [TestCase("/**/Grandchild")]
        [TestCase("**/Grandchild")]
        public void MatchPath_Match(string grob)
        {
            var grandchild = CreateThreeGenerationObjects();
            var actual = grandchild.transform.MatchPath(grob);
            Assert.That(actual, Is.True);
        }

        [TestCase("/Parent/*/Child/Grandchild")]
        [TestCase("/Parent/**/Child/Grandchild")]
        [TestCase("*/Grandchild")]
        [TestCase("**/Granddaughter")]
        public void MatchPath_NotMatch(string grob)
        {
            var grandchild = CreateThreeGenerationObjects();
            var actual = grandchild.transform.MatchPath(grob);
            Assert.That(actual, Is.False);
        }

        [TestCase("**/Grandchild*")]
        [TestCase("**/Gran?child")]
        [TestCase("*")]
        [TestCase("**")]
        public void MatchPath_InvalidGrobPattern_ThrowsArgumentException(string grob)
        {
            var grandchild = CreateThreeGenerationObjects();
            Assert.That(() => grandchild.transform.MatchPath(grob),
                Throws.TypeOf<ArgumentException>()
                    .And.Message.StartsWith("Wildcards cannot be used in the most right section of path"));
        }
    }
}
