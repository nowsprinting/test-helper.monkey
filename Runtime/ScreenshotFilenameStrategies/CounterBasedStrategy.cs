// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Runtime.CompilerServices;

namespace TestHelper.Monkey.ScreenshotFilenameStrategies
{
    /// <summary>
    ///     Sequential number based screenshot file path strategy.
    /// </summary>
    public class CounterBasedStrategy : AbstractPrefixAndUniqueIDStrategy
    {
        private int _count;


        public CounterBasedStrategy(string filenamePrefix, [CallerMemberName] string callerMemberName = null) : base(filenamePrefix, callerMemberName)
        {
        }


        protected override string GetUniqueID()
        {
            return (++_count).ToString("D04");
        }
    }
}
