// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using NUnit.Framework;
using TestHelper.Monkey.Annotations.Enums;
using TestHelper.Random;

namespace TestHelper.Monkey.Random
{
    [TestFixture]
    public class RandomStringTest
    {
        [Test]
        public void NextDigits()
        {
            // Property-based testing
            for (var i = 0; i < 1000; i++)
            {
                var random = new RandomWrapper(i); // Make deterministic, and choose a number from wide range
                var randomString = new RandomStringImpl(random);
                var result = randomString.Next(new RandomStringParameters(5, 10, CharactersKind.Digits));
                Assert.That(result.Length, Is.GreaterThanOrEqualTo(5).And.LessThanOrEqualTo(10));
                Assert.That(result.All(c => char.IsDigit(c)), Is.True);
            }
        }

        [Test]
        public void NextAlphanumeric()
        {
            // Property-based testing
            for (var i = 0; i < 1000; i++)
            {
                var random = new RandomWrapper(i); // Make deterministic, and choose a number from wide range
                var randomString = new RandomStringImpl(random);
                var result = randomString.Next(new RandomStringParameters(5, 10, CharactersKind.Alphanumeric));
                Assert.That(result.Length, Is.GreaterThanOrEqualTo(5).And.LessThanOrEqualTo(10));
                Assert.That(result.All(c => char.IsLetterOrDigit(c)), Is.True);
            }
        }

        [Test]
        public void NextPrintable()
        {
            // Property-based testing
            for (var i = 0; i < 1000; i++)
            {
                var random = new RandomWrapper(i); // Make deterministic, and choose a number from wide range
                var randomString = new RandomStringImpl(random);
                var result = randomString.Next(new RandomStringParameters(5, 10, CharactersKind.Printable));
                Assert.That(result.Length, Is.GreaterThanOrEqualTo(5).And.LessThanOrEqualTo(10));
                Assert.That(result.All(c => !char.IsControl(c)), Is.True);
            }
        }
    }
}
