// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.Operators;

namespace TestHelper.Monkey.Samples.UGUIDemo
{
    [TestFixture]
    public class ScenarioTest
    {
        private readonly GameObjectFinder _finder = new GameObjectFinder(3.0d);
        private readonly MonkeyConfig _config = new MonkeyConfig();

        [TestCase("Profile")]
        [TestCase("Difficulty")]
        [TestCase("Help")]
        [TestCase("Credit")]
        [LoadScene("../../Scenes/uGUIDemo.unity")]
        [TakeScreenshot]
        public async Task OpenSubScreens(string target)
        {
            // Wait for the title screen to be displayed.
            await _finder.FindByNameAsync("Title");

            // When click Start button, then open Home screen.
            var startButton = await _finder.FindByNameAsync("StartButton", interactable: true);
            var startButtonObject = startButton.GameObject;
            var startOperator = startButtonObject.SelectOperators<IClickOperator>(_config.Operators).First();
            await startOperator.OperateAsync(startButtonObject, startButton.RaycastResult);

            await _finder.FindByNameAsync("Home");

            // When click target button, then open target screen.
            var targetButton = await _finder.FindByNameAsync($"{target}Button", interactable: true);
            var targetObject = targetButton.GameObject;
            var targetOperator = targetObject.SelectOperators<IClickOperator>(_config.Operators).First();
            await targetOperator.OperateAsync(targetObject, targetButton.RaycastResult);

            await _finder.FindByNameAsync(target);

            // When click Back button, then return Home screen.
            var backButton = await _finder.FindByPathAsync($"**/{target}/BackButton", interactable: true);
            var backObject = backButton.GameObject;
            var backOperator = backObject.SelectOperators<IClickOperator>(_config.Operators).First();
            await backOperator.OperateAsync(backObject, backButton.RaycastResult);

            await _finder.FindByNameAsync("Home");
        }
    }
}
