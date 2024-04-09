// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.Operators;
using UnityEngine;
using UnityEngine.UI;

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
        [LoadScene("Assets/Samples/Monkey Test Helper/0.11.0/uGUI Demo/Scenes/uGUIDemo.unity")]
        [TakeScreenshot]
        public async Task OpenSubScreens(string target)
        {
            // Wait for the title screen to be displayed.
            await _finder.FindByNameAsync("Title");

            // When click Start button, then open Home screen.
            var startButton = await _finder.FindByNameAsync("StartButton", interactable: true);
            var startComponent = CreateInteractableComponent(startButton.GetComponent<Button>());
            var startClickOperator = startComponent.GetOperatorsByType(OperatorType.Click).First();
            startClickOperator.OperateAsync(startComponent.component);

            await _finder.FindByNameAsync("Home");

            // When click target button, then open target screen.
            var targetButton = await _finder.FindByNameAsync($"{target}Button", interactable: true);
            var targetComponent = CreateInteractableComponent(targetButton.GetComponent<Button>());
            var targetClickOperator = targetComponent.GetOperatorsByType(OperatorType.Click).First();
            targetClickOperator.OperateAsync(targetComponent.component);

            await _finder.FindByNameAsync(target);

            // When click Back button, then return Home screen.
            var backButton = await _finder.FindByPathAsync($"**/{target}/BackButton", interactable: true);
            var backComponent = CreateInteractableComponent(backButton.GetComponent<Button>());
            var backClickOperator = backComponent.GetOperatorsByType(OperatorType.Click).First();
            backClickOperator.OperateAsync(backComponent.component);

            await _finder.FindByNameAsync("Home");
        }

        private InteractiveComponent CreateInteractableComponent(MonoBehaviour component)
        {
            return InteractiveComponent.CreateInteractableComponent(component, operators: _config.Operators);
        }
    }
}
