// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.IO;
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
        /// Directory path to save screenshots.
        /// Default save path is <c>Application.persistentDataPath</c> + "/TestHelper.Monkey/Screenshots/".
        /// </summary>
        public string Directory { get; set; } = GetDefaultDirectory();
        
        /// <summary>
        /// Strategy for file paths of screenshot images.
        /// </summary>
        public IScreenshotFilenameStrategy FilenameStrategy { get; set; } = new CounterBasedStrategy(null);

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
        

        /// <summary>
        /// Returns a default directory for screenshot images
        /// </summary>
        /// <returns>Default directory for screenshot images</returns>
        public static string GetDefaultDirectory()
        {
            return Path.Combine(Application.persistentDataPath, "TestHelper.Monkey", "Screenshots");
        }
    }
}
