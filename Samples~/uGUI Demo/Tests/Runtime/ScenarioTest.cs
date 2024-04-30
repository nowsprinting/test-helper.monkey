// Copyright (c) 2023-2024 Koji Hasegawa.
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
            var startComponent = startButton.GetInteractableComponents().First();
            var startOperator = startComponent.SelectOperators<IClickOperator>(_config.Operators).First();
            startOperator.OperateAsync(startComponent);

            await _finder.FindByNameAsync("Home");

            // When click target button, then open target screen.
            var targetButton = await _finder.FindByNameAsync($"{target}Button", interactable: true);
            var targetComponent = targetButton.GetInteractableComponents().First();
            var targetOperator = targetComponent.SelectOperators<IClickOperator>(_config.Operators).First();
            targetOperator.OperateAsync(targetComponent);

            await _finder.FindByNameAsync(target);

            // When click Back button, then return Home screen.
            var backButton = await _finder.FindByPathAsync($"**/{target}/BackButton", interactable: true);
            var backComponent = backButton.GetInteractableComponents().First();
            var backOperator = backComponent.SelectOperators<IClickOperator>(_config.Operators).First();
            backOperator.OperateAsync(backComponent);

            await _finder.FindByNameAsync("Home");
        }
    }
}
