// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using TestHelper.Monkey.ScreenshotFilenameStrategies;
using UnityEngine;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Take screenshots options.
    /// </summary>
    public class ScreenshotOptions
    {
        /// <summary>
        /// Strategy for file paths of screenshot images.
        /// </summary>
        public Func<FilePath> FilePathStrategy { get; set; } = CounterBasedStrategy.Create(Counter.Global);

        /// <summary>
        /// The factor to increase resolution with.
        /// SuperSize and StereoCaptureMode cannot be specified at the same time.
        /// </summary>
        public int SuperSize { get; set; } = 1;

        /// <summary>
        /// The eye texture to capture when stereo rendering is enabled.
        /// SuperSize and StereoCaptureMode cannot be specified at the same time.
        /// </summary>
        /// <remarks>
        /// Require stereo rendering settings.
        /// See: https://docs.unity3d.com/Manual/SinglePassStereoRendering.html
        /// </remarks>
        public ScreenCapture.StereoScreenCaptureMode StereoCaptureMode { get; set; } =
            ScreenCapture.StereoScreenCaptureMode.LeftEye;
        
        
        public readonly struct FilePath
        {
            public readonly string Directory;
            public readonly string Filename;


            public FilePath(string directory, string filename)
            {
                Directory = directory;
                Filename = filename;
            }
        }
    }
}
