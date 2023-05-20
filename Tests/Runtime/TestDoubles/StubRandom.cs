// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using NUnit.Framework;
using TestHelper.Monkey.Random;

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
    }
}
