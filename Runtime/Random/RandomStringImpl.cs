// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Text;
using TestHelper.Random;
using UnityEngine.Assertions;

namespace TestHelper.Monkey.Random
{
    public class RandomStringImpl : IRandomString
    {
        internal const string CharsASCIIDigits = "0123456789";
        internal const string CharsASCIILowers = "abcdefghijklmnopqrstuvwxyz";
        internal const string CharsASCIIUppers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        internal const string CharsASCIISymbols = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
        internal const string CharsASCIIAlphanumeric = CharsASCIIDigits + CharsASCIILowers + CharsASCIIUppers;
        internal const string CharsASCIIPrintable = CharsASCIIAlphanumeric + CharsASCIISymbols;

        private static StringBuilder _sb = new StringBuilder();
        private readonly IRandom _random;


        /// <summary>
        /// Initializes a new instance of the <c>RandomStringImpl</c> class,
        /// using the specified random number generator.
        /// <remarks>This class is not thread-safe.</remarks>
        /// </summary>
        /// <param name="random">Random number generator</param>
        public RandomStringImpl(IRandom random)
        {
            _random = random;
        }


        /// <summary>
        /// Returns a random string.
        /// <remarks>This method is not thread safe.</remarks>
        /// </summary>
        /// <param name="parameters">String generation parameters</param>
        /// <returns>Generated string</returns>
        public string Next(RandomStringParameters parameters)
        {
            Assert.IsTrue(parameters.MinimumLength <= parameters.MaximumLength);
            var len = parameters.MinimumLength + _random.Next(parameters.MaximumLength - parameters.MinimumLength + 1);

            var chars = GetCharsByKind(parameters.CharactersKind);

            _sb.Clear();
            for (var i = 0; i < len; i++)
            {
                _sb.Append(chars[_random.Next(chars.Length)]);
            }

            return _sb.ToString();
        }


        private static string GetCharsByKind(CharactersKind charactersKind)
        {
            switch (charactersKind)
            {
                case CharactersKind.Digits:
                    return CharsASCIIDigits;
                case CharactersKind.Alphanumeric:
                    return CharsASCIIAlphanumeric;
                case CharactersKind.Printable:
                    return CharsASCIIPrintable;
                default:
                    throw new ArgumentOutOfRangeException(nameof(charactersKind), charactersKind, null);
            }
        }
    }
}
