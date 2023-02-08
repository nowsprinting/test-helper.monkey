// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

namespace TestHelper.Monkey.Random
{
    /// <summary>
    /// Reference implementation of Random class using <c>System.Random</c>.
    /// </summary>
    public class RandomImpl : IRandom
    {
        private readonly System.Random _random;
        private readonly int _seed;

        /// <summary>
        /// Initializes a new instance of the <c>RandomImpl</c> class with seed value.
        /// </summary>
        /// <param name="seed">random seed</param>
        public RandomImpl(int seed)
        {
            _random = new System.Random(seed);
            _seed = seed;
        }

        /// <inheritdoc />
        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"System.Random, seed={_seed}";
        }
    }
}
