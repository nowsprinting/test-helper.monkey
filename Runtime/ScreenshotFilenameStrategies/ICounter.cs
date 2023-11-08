// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

namespace TestHelper.Monkey.ScreenshotFilenameStrategies
{
    /// <summary>
    /// An interface for counters
    /// </summary>
    public interface ICounter
    {
        /// <summary>
        /// Get a count and increment the count.
        /// </summary>
        /// <returns></returns>
        int GetCountAndIncrement();
    }
}
