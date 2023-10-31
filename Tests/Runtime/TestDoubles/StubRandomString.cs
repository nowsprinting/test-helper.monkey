// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using NUnit.Framework;
using TestHelper.Monkey.Random;

namespace TestHelper.Monkey.TestDoubles
{
    public class StubRandomString : IRandomString
    {
        private readonly string[] _returnValues;
        private int _returnValueIndex;


        public StubRandomString(params string[] returnValues)
        {
            Assert.IsNotEmpty(returnValues);
            _returnValues = returnValues;
            _returnValueIndex = 0;
        }


        public string Next(RandomStringParameters _)
        {
            if (_returnValues.Length <= _returnValueIndex)
            {
                throw new ArgumentException("The number of calls exceeds the length of arguments.");
            }

            return _returnValues[_returnValueIndex++];
        }
    }
}
