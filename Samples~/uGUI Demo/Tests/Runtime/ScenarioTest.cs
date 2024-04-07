// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
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
        [LoadScene("Assets/Samples/Monkey Test Helper/0.10.0/uGUI Demo/Scenes/uGUIDemo.unity")]
        [TakeScreenshot]
        public async Task OpenSubScreens(string target)
        {
            // Wait for the title screen to be displayed.
            await _finder.FindByNameAsync("Title");

            // When click Start button, then open Home screen.
            var startButton = await _finder.FindByNameAsync("StartButton", interactable: true);
            var startComponent = CreateInteractableComponent(startButton.GetComponent<Button>());
            Assume.That(startComponent.CanClick(), Is.True);
            startComponent.Click();
            await _finder.FindByNameAsync("Home");

            // When click target button, then open target screen.
            var targetButton = await _finder.FindByNameAsync($"{target}Button", interactable: true);
            var targetComponent = CreateInteractableComponent(targetButton.GetComponent<Button>());
            Assume.That(targetComponent.CanClick(), Is.True);
            targetComponent.Click();
            await _finder.FindByNameAsync(target);

            // When click Back button, then return Home screen.
            var backButton = await _finder.FindByPathAsync($"**/{target}/BackButton", interactable: true);
            var backComponent = CreateInteractableComponent(backButton.GetComponent<Button>());
            Assume.That(backComponent.CanClick(), Is.True);
            backComponent.Click();
            await _finder.FindByNameAsync("Home");
        }

        private InteractiveComponent CreateInteractableComponent(MonoBehaviour component)
        {
            return InteractiveComponent.CreateInteractableComponent(component, operators: _config.Operators);
        }
    }
}
