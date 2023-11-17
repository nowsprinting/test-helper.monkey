// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

namespace TestHelper.Monkey.ScreenshotFilenameStrategies
{
    public interface IScreenshotFileNameStrategy
    {
        /// <summary>
        /// Get a file path for screenshots
        /// </summary>
        /// <returns>A generated file path</returns>
        string GetFileName();
    }
}
