// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

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
        public string Directory { get; set; } = null;

        /// <summary>
        /// Prefix of screenshots filename.
        /// Default prefix is <c>CurrentTest.Name</c> when run in test-framework context.
        /// Using caller method name when run in runtime context.
        /// </summary>
        public string FilenamePrefix { get; set; } = null;

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
