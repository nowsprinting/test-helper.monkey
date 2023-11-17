// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey.ScreenshotFilenameStrategies;

namespace TestHelper.Monkey.TestDoubles
{
    public class StubScreenshotFileNameStrategy : IScreenshotFileNameStrategy
    {
        public string ScreenshotFileName;


        public StubScreenshotFileNameStrategy(string screenshotFileName)
        {
            ScreenshotFileName = screenshotFileName;
        }


        public string GetFileName()
        {
            return ScreenshotFileName;
        }
    }
}
