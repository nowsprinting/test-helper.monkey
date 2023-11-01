// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;

namespace TestHelper.Monkey.Random
{
    /// <summary>
    /// Reference implementation of <c>IRandom</c>, wrapping <c>System.Random</c>.
    /// </summary>
    /// <seealso cref="System.Random"/>
    [Obsolete("Use TestHelper.Random package")]
    public class RandomImpl : IRandom
    {
        private readonly System.Random _random;
        private readonly int _seed;

        /// <summary>
        /// Initializes a new instance of the <c>RandomImpl</c> class,
        /// using the specified seed value.
        /// </summary>
        /// <param name="seed">A number used to calculate a starting value for the pseudo-random number sequence. If a negative number is specified, the absolute value of the number is used.</param>
        public RandomImpl(int seed)
        {
            _random = new System.Random(seed);
            _seed = seed;
        }

        /// <summary>
        /// Initializes a new instance of the <c>RandomImpl</c> class,
        /// using <c>Environment.TickCount</c> to seed value.
        /// </summary>
        public RandomImpl() : this(Environment.TickCount) { }

        /// <inheritdoc />
        public virtual int Next()
        {
            return _random.Next();
        }

        /// <inheritdoc />
        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"wrapping System.Random, seed={_seed}";
        }
    }
}
