// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

namespace TestHelper.Monkey.Random
{
    /// <summary>
    /// Reference implementation of Random class using System.Random
    /// </summary>
    public class RandomImpl : IRandom
    {
        private readonly System.Random _random;
        private readonly int _seed;

        public RandomImpl(int seed)
        {
            _random = new System.Random(seed);
            _seed = seed;
        }

        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }

        public override string ToString()
        {
            return $"System.Random, seed={_seed}";
        }
    }
}
