// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

namespace TestHelper.Monkey.ScreenshotFilenameStrategies
{
    /// <summary>
    /// Counter class.
    /// </summary>
    public class Counter : ICounter
    {
        private int _count = 1;
        
        /// <inheritdoc cref="ICounter.GetCountAndIncrement" />
        public int GetCountAndIncrement()
        {
            return _count++;
        }

        /// <summary>
        /// Global counter.
        /// </summary>
        public static readonly ICounter Global = new Counter();
    }
}
