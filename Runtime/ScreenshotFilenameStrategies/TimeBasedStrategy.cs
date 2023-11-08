// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Runtime.CompilerServices;

namespace TestHelper.Monkey.ScreenshotFilenameStrategies
{
    /// <summary>
    ///     Time based screenshot file path strategy.
    /// </summary>
    public static class TimeBasedStrategy
    {
        /// <summary>
        ///     Create a file path strategy that is based on the time as the unique identifier.
        /// </summary>
        /// <param name="directory">
        ///     Directory for screenshot images. If null or empty, use returned one from
        ///     <c cref="ScreenshotFilenameStrategy.GetDefaultDirectory" />
        /// </param>
        /// <param name="filenamePrefix">
        ///     Directory for screenshot images. If null or empty, use returned one from
        ///     <c cref="ScreenshotFilenameStrategy.GetDefaultFilenamePrefix" />
        /// </param>
        /// <returns>
        ///     File path such as <c>$"{directory}/{filenamePrefix}_1970-01-01_00_00_00.0000000.png"</c> (path
        ///     separators will be the platform specific one of course)
        /// </returns>
        public static Func<ScreenshotOptions.FilePath> Create(
            string directory = null,
            string filenamePrefix = null,
            // ReSharper disable once InvalidXmlDocComment
            Func<DateTime> now = null,
            // ReSharper disable once InvalidXmlDocComment
            [CallerMemberName] string callerMemberName = null
        )
        {
            if (now == null)
            {
                now = () => DateTime.Now;
            }
            return () => ScreenshotFilenameStrategy.CreateFilePath(
                directory,
                filenamePrefix,
                callerMemberName,
                now().ToString("O").Replace(":", "_")
            );
        }
    }
}
