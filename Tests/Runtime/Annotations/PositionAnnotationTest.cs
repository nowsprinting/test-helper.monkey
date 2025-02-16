// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.DefaultStrategies;
using UnityEngine;

namespace TestHelper.Monkey.Annotations
{
    [TestFixture]
    public class PositionAnnotationTest
    {
        private const string TestScene = "../../Scenes/PositionAnnotations.unity";

        [Test]
        [LoadScene(TestScene)]
        public void AttachAnnotation_CorrectedToBeReachable(
            [Values(
                "WorldOffsetAnnotation",
                "ScreenOffsetAnnotation",
                "WorldPositionAnnotation",
                "ScreenPositionAnnotation"
            )]
            string name
        )
        {
            var target = GameObject.Find(name);

            // Without no position annotations, IsReachable() is always false because
            // gameObject.transform.position is not in the mesh. So IsReachable() is true means
            // the position annotation work well
            Assert.That(DefaultReachableStrategy.IsReachable(target), Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void DisabledAnnotation_NotReachable(
            [Values(
                "WorldOffsetAnnotation",
                "ScreenOffsetAnnotation",
                "WorldPositionAnnotation",
                "ScreenPositionAnnotation"
            )]
            string name
        )
        {
            var target = GameObject.Find(name);
            var annotation = target.GetComponents<MonoBehaviour>().First(x => x.GetType().Name.Equals(name));
            annotation.enabled = false;

            Assert.That(DefaultReachableStrategy.IsReachable(target), Is.False);
        }
    }
}
