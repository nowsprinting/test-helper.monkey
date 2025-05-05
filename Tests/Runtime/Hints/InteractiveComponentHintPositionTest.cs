// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.DefaultStrategies;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools.Utils;

namespace TestHelper.Monkey.Hints
{
    [TestFixture]
    public class InteractiveComponentHintPositionTest
    {
        private const string TestScene = "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Hints.unity";

        [TestCase("Left Cube", -1f, 0, 0)]
        [TestCase("Right Cube", 2f, 0, 0)]
        [TestCase("Left Cube", -1f, 0, 0)]
        [TestCase("Right Cube", 2f, 0, 0)]
        [LoadScene(TestScene)]
        public void Get(string targetName, float x, float y, float z)
        {
            var camera = Camera.main;
            var target = SceneManager
                .GetActiveScene()
                .GetRootGameObjects()
                .First(go => go.name == targetName);
            var screenPoint = DefaultScreenPointStrategy.GetScreenPoint(target);

            var actual = InteractiveComponentHintPosition.GetWorldPoint(camera, screenPoint, target.transform.position);
            var expected = new Vector3(x, y, z);
            var msg = $"Actual: {actual.ToString("F4")}\nExpected: {expected.ToString("F4")}";
            Assert.That(actual, Is.EqualTo(expected).Using(Vector3EqualityComparer.Instance), msg);
        }
    }
}
