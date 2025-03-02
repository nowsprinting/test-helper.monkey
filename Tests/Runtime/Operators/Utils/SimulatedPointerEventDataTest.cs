// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TestHelper.Monkey.Operators.Utils
{
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP017:Prefer using")]
    public class SimulatedPointerEventDataTest
    {
        [Test]
        [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
        public void Constructor_WithoutPointingDeviceTypeInEditor_AsMouse()
        {
            var sut = new SimulatedPointerEventData(null, default);
            Assert.That(sut.pointerId, Is.EqualTo(-1));
            sut.Dispose();
        }

        [Test]
        public void Constructor_WithMouse_SetPointerIdIsLeftButton()
        {
            var sut = new SimulatedPointerEventData(null, default,
                SimulatedPointerEventData.PointingDeviceType.Mouse);
            Assert.That(sut.pointerId, Is.EqualTo(-1));
            sut.Dispose();
        }

        [Test]
        public void Constructor_WithTouchScreen_IncrementTouchCount()
        {
            var firstTouch = new SimulatedPointerEventData(null, default,
                SimulatedPointerEventData.PointingDeviceType.TouchScreen);
            Assume.That(firstTouch.pointerId, Is.EqualTo(0));

            var secondTouch = new SimulatedPointerEventData(null, default,
                SimulatedPointerEventData.PointingDeviceType.TouchScreen);
            Assert.That(secondTouch.pointerId, Is.EqualTo(1));

            firstTouch.Dispose();
            secondTouch.Dispose();
        }

        [Test]
        public void Dispose_WithTouchScreen_DecrementTouchCount()
        {
            var firstTouch = new SimulatedPointerEventData(null, default,
                SimulatedPointerEventData.PointingDeviceType.TouchScreen);
            Assume.That(firstTouch.pointerId, Is.EqualTo(0));
            firstTouch.Dispose();

            var secondTouch = new SimulatedPointerEventData(null, default,
                SimulatedPointerEventData.PointingDeviceType.TouchScreen);
            Assert.That(secondTouch.pointerId, Is.EqualTo(0));
            secondTouch.Dispose();
        }
    }
}
