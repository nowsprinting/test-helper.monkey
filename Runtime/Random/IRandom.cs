// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

namespace TestHelper.Monkey.Random
{
    /// <summary>
    /// Random number generator interface like a <c>System.Random</c> class.
    /// </summary>
    /// <seealso cref="System.Random"/>
    public interface IRandom
    {
        /// <summary>
        /// Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 32-bit signed integer greater than or equal to zero and less than <c>int.MaxValue</c>.</returns>
        int Next();

        /// <summary>
        /// Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. maxValue must be greater than or equal to zero.</param>
        /// <returns>A 32-bit signed integer greater than or equal to zero, and less than maxValue; that is, the range of return values ordinarily includes zero but not maxValue. However, if maxValue equals zero, maxValue is returned.</returns>
        int Next(int maxValue);
    }
}
