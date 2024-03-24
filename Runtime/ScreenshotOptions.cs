// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey.ScreenshotFilenameStrategies;
using TestHelper.RuntimeInternals;
using UnityEngine;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Take screenshots options.
    /// </summary>
    public class ScreenshotOptions
    {
        /// <summary>
        /// Directory to save screenshots.
        /// If omitted, the directory specified by command line argument "-testHelperScreenshotDirectory" is used.
        /// If the command line argument is also omitted, <c>Application.persistentDataPath</c> + "/TestHelper/Screenshots/" is used.
        /// </summary>
        public string Directory { get; set; } = CommandLineArgs.GetScreenshotDirectory();

        /// <summary>
        /// Strategy for file paths of screenshot images.
        /// </summary>
        public IScreenshotFilenameStrategy FilenameStrategy { get; set; } = new CounterBasedStrategy();

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
    }
}
