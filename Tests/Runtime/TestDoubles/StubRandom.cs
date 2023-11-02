// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using NUnit.Framework;
using TestHelper.Random;
using UnityEngine;

namespace TestHelper.Monkey.TestDoubles
{
    public class StubRandom : IRandom
    {
        private readonly int[] _returnValues;
        private int _returnValueIndex;

        public StubRandom(params int[] returnValues)
        {
            Assert.IsNotEmpty(returnValues);
            _returnValues = returnValues;
            _returnValueIndex = 0;
        }

        public int Next()
        {
            if (_returnValues.Length <= _returnValueIndex)
            {
                throw new ArgumentException("The number of calls exceeds the length of arguments.");
            }

            return _returnValues[_returnValueIndex++];
        }

        public int Next(int maxValue)
        {
            if (_returnValues.Length <= _returnValueIndex)
            {
                throw new ArgumentException("The number of calls exceeds the length of arguments.");
            }

            return _returnValues[_returnValueIndex++];
        }

        public int Next(int minValue, int maxValue)
        {
            throw new NotImplementedException();
        }

        public void NextBytes(byte[] buffer)
        {
            throw new NotImplementedException();
        }

#if UNITY_2021_2_OR_NEWER
        public void NextBytes(Span<byte> buffer)
        {
            throw new NotImplementedException();
        }
#endif

        public double NextDouble()
        {
            throw new NotImplementedException();
        }

        public float Range(float minInclusive, float maxInclusive)
        {
            throw new NotImplementedException();
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            throw new NotImplementedException();
        }

        public int RandomRangeInt(int minInclusive, int maxExclusive)
        {
            throw new NotImplementedException();
        }

        public float value()
        {
            throw new NotImplementedException();
        }

        public Vector3 insideUnitSphere()
        {
            throw new NotImplementedException();
        }

        public Vector2 insideUnitCircle()
        {
            throw new NotImplementedException();
        }

        public Vector3 onUnitSphere()
        {
            throw new NotImplementedException();
        }

        public Quaternion rotation()
        {
            throw new NotImplementedException();
        }

        public Quaternion rotationUniform()
        {
            throw new NotImplementedException();
        }

        public Color ColorHSV(float hueMin = 0, float hueMax = 1, float saturationMin = 0, float saturationMax = 1,
            float valueMin = 0, float valueMax = 1, float alphaMin = 1, float alphaMax = 1)
        {
            throw new NotImplementedException();
        }
    }
}
