// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;

namespace TestHelper.Monkey.Exceptions
{
    /// <summary>
    /// Detected multiple <c>GameObject</c>s matching the specified criteria.
    /// </summary>
    public class MultipleGameObjectsMatchingException : ApplicationException
    {
        public MultipleGameObjectsMatchingException(string message) : base(message) { }
        public MultipleGameObjectsMatchingException() : base() { }
    }
}
