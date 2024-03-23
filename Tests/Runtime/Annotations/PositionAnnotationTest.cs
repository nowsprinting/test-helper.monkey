// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.ScreenPointStrategies;

namespace TestHelper.Monkey.Annotations
{
    [TestFixture]
    public class PositionAnnotationTest
    {
        private const string TestScene = "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/PositionAnnotations.unity";

        [Test]
        [LoadScene(TestScene)]
        public void IsReallyInteractive(
            [Values(
                "WorldOffsetAnnotation",
                "ScreenOffsetAnnotation",
                "WorldPositionAnnotation",
                "ScreenPositionAnnotation"
            )]
            string name
        )
        {
            var target = InteractiveComponentCollector
                .FindInteractableComponents()
                .First(x => x.gameObject.name == name);

            // Without no position annotations, IsReallyInteractiveFromUser() is always false because
            // gameObject.transform.position is not in the mesh. So IsReallyInteractiveFromUser() is true means
            // the position annotation work well
            Assert.That(target.IsReallyInteractiveFromUser(DefaultScreenPointStrategy.GetScreenPoint), Is.True);
        }
    }
}
