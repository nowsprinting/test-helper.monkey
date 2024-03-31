// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;

namespace TestHelper.Monkey.Samples.UGUIDemo
{
    [TestFixture]
    public class ScenarioTest
    {
        private readonly GameObjectFinder _finder = new GameObjectFinder(3.0d);

        [TestCase("Profile")]
        [TestCase("Difficulty")]
        [TestCase("Help")]
        [TestCase("Credit")]
        [LoadScene("Assets/Samples/Monkey Test Helper/0.9.0/uGUI Demo/Scenes/uGUIDemo.unity")]
        [TakeScreenshot]
        public async Task OpenSubScreens(string target)
        {
            // Wait for the title screen to be displayed.
            await _finder.FindByNameAsync("Title");

            // When click Start button, then open Home screen.
            var startButton = await _finder.FindByNameAsync("StartButton", interactable: true);
            InteractiveComponent.CreateInteractableComponent(startButton).Click();
            await _finder.FindByNameAsync("Home");

            // When click target button, then open target screen.
            var targetButton = await _finder.FindByNameAsync($"{target}Button", interactable: true);
            InteractiveComponent.CreateInteractableComponent(targetButton).Click();
            await _finder.FindByNameAsync(target);

            // When click Back button, then return Home screen.
            var backButton = await _finder.FindByPathAsync($"**/{target}/BackButton", interactable: true);
            InteractiveComponent.CreateInteractableComponent(backButton).Click();
            await _finder.FindByNameAsync("Home");
        }
    }
}
