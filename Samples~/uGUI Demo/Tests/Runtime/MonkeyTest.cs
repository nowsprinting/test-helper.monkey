// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;

namespace TestHelper.Monkey.Samples.UGUIDemo
{
    [TestFixture]
    [GameViewResolution(GameViewResolution.VGA)]
    public class MonkeyTest
    {
        [Test]
        [LoadScene("Assets/Samples/Monkey Test Helper/0.9.0/uGUI Demo/Scenes/uGUIDemo.unity")]
        public async Task Run()
        {
            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromSeconds(10), // Run 10 seconds
                Screenshots = new ScreenshotOptions() // Take screenshots
            };
            await Monkey.Run(config);
        }
    }
}
