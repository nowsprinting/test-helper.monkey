// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;

namespace TestHelper.Monkey.Exceptions
{
    /// <summary>
    /// Detected an infinite loop in monkey testing.
    /// </summary>
    public class InfiniteLoopException : ApplicationException
    {
        public InfiniteLoopException(string message) : base(message) { }
        public InfiniteLoopException() : base() { }
    }
}
