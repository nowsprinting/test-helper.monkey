// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey.Annotations.Enums;

namespace TestHelper.Monkey.Random
{
    /// <summary>
    /// Parameters for random string generation.
    /// </summary>
    public readonly struct RandomStringParameters
    {
        /// <summary>
        /// Minimum length of generated strings. Length of the strings must be greater than or equal the value.
        /// </summary>
        public readonly int MinimumLength;

        /// <summary>
        /// Maximum length of generated strings. Length of the strings must be lower than or equal the value.
        /// </summary>
        public readonly int MaximumLength;

        /// <summary>
        /// Character kind.
        /// </summary>
        public readonly CharactersKind CharactersKind;


        /// <summary>
        /// Creates a new parameters.
        /// </summary>
        /// <param name="minimumLength">Minimum length</param>
        /// <param name="maximumLength">Maximum length</param>
        /// <param name="charactersKind">Character kind allowed</param>
        public RandomStringParameters(int minimumLength, int maximumLength, CharactersKind charactersKind)
        {
            MinimumLength = minimumLength;
            MaximumLength = maximumLength;
            CharactersKind = charactersKind;
        }


        /// <summary>
        /// Default random string generation parameters.
        /// </summary>
        public static readonly RandomStringParameters Default = new RandomStringParameters(
            5,
            10,
            CharactersKind.Alphanumeric
        );
    }
}
