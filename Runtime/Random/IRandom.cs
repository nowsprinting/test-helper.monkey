// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

namespace TestHelper.Monkey.Random
{
    /// <summary>
    /// Random number generator interface
    /// </summary>
    public interface IRandom
    {
        /// <summary>
        /// Returns random number that is less than specified max-value.
        /// </summary>
        /// <param name="maxValue">Upper bound of the random number to be generated</param>
        /// <returns>Generated random number</returns>
        int Next(int maxValue);
    }
}
