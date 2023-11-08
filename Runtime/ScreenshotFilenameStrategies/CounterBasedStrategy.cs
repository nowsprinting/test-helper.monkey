// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Runtime.CompilerServices;

namespace TestHelper.Monkey.ScreenshotFilenameStrategies
{
    /// <summary>
    ///     Sequential number based screenshot file path strategy.
    /// </summary>
    public static class CounterBasedStrategy
    {
        /// <summary>
        ///     Create a file path strategy that is based on sequential number as the unique identifier.
        /// </summary>
        /// <param name="directory">
        ///     Directory for screenshot images. If null or empty, use returned one from
        ///     <c cref="ScreenshotFilenameStrategy.GetDefaultDirectory" />
        /// </param>
        /// <param name="filenamePrefix">
        ///     Directory for screenshot images. If null or empty, use returned one from
        ///     <c cref="ScreenshotFilenameStrategy.GetDefaultFilenamePrefix" />
        /// </param>
        /// <param name="counter">A counter to get sequential numbers</param>
        /// <returns>
        ///     File path such as <c>$"{directory}/{filenamePrefix}_{i++:D010}"</c> (path separators will be the platform
        ///     specific one of course)
        /// </returns>
        public static Func<ScreenshotOptions.FilePath> Create(
            ICounter counter,
            string directory = null,
            string filenamePrefix = null,
            // ReSharper disable once InvalidXmlDocComment
            [CallerMemberName] string callerMemberName = null
        )
        {
            return () => ScreenshotFilenameStrategy.CreateFilePath(
                directory,
                filenamePrefix,
                callerMemberName,
                counter.GetCountAndIncrement().ToString("D04")
            );
        }
    }
}
