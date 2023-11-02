// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

namespace TestHelper.Monkey.Random
{
    public interface IRandomString
    {
        string Next(RandomStringParameters parameters);
    }
}
