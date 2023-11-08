// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TestHelper.Monkey.ScreenshotFilenameStrategies
{
    /// <summary>
    ///     Frame count based screenshot file path strategy.
    /// </summary>
    public static class FrameCountBasedStrategy
    {
        /// <summary>
        ///     Create a file path strategy that is based on the time as the unique identifier.
        /// </summary>
        /// <param name="directory">
        ///     Directory for screenshot images. If null or empty, use returned one from
        ///     <c cref="ScreenshotFilenameStrategy.GetDefaultDirectory" />
        /// </param>
        /// <param name="filenamePrefix">
        ///     Filename prefix for screenshot images. If null or empty, use returned one from
        ///     <c cref="ScreenshotFilenameStrategy.GetDefaultFilenamePrefix" />
        /// </param>
        /// <returns>
        ///     File path such as <c>$"{directory}/{filenamePrefix}_{Time.frameCount:D010}"</c> (path separators will be the
        ///     platform specific one of course)
        /// </returns>
        public static Func<ScreenshotOptions.FilePath> Create(
            string directory = null,
            string filenamePrefix = null,
            // ReSharper disable once InvalidXmlDocComment
            Func<int> frameCount = null,
            // ReSharper disable once InvalidXmlDocComment
            [CallerMemberName] string callerMemberName = null
        )
        {
            if (frameCount == null)
            {
                frameCount = () => Time.frameCount;
            }
            return () => ScreenshotFilenameStrategy.CreateFilePath(
                directory,
                filenamePrefix,
                callerMemberName,
                frameCount().ToString("D010")
            );
        }
    }
}
