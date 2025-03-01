// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.Annotations;
using UnityEngine;

namespace TestHelper.Monkey.DefaultStrategies
{
    [TestFixture]
    public class DefaultIgnoreStrategyTest
    {
        private const string TestScene = "../../Scenes/PhysicsRaycasterSandbox.unity";

        [Test]
        [LoadScene(TestScene)]
        public void IsIgnored_WithAnnotation_Ignored()
        {
            var cube = GameObject.Find("Cube");
            cube.AddComponent<IgnoreAnnotation>();

            Assert.That(new DefaultIgnoreStrategy().IsIgnored(cube), Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void IsIgnored_WithDisabledAnnotation_NotIgnored()
        {
            var cube = GameObject.Find("Cube");
            var annotation = cube.AddComponent<IgnoreAnnotation>();
            annotation.enabled = false;

            Assert.That(new DefaultIgnoreStrategy().IsIgnored(cube), Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void IsIgnored_WithoutAnnotation_NotIgnored()
        {
            var cube = GameObject.Find("Cube");
            Assert.That(new DefaultIgnoreStrategy().IsIgnored(cube), Is.False);
        }
    }
}
