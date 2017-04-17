using System;
using Xunit;

namespace DupImageLib.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var hash = ImageHashes.CalculateMedianHash64(@"C:\Users\jptei\Dropbox\Pictures\Wallpapers Additional\00a10f6575cf41bce326f03bf5482fc2440727c4.jpg");

            Assert.NotEqual(hash, 0L);
        }
    }
}
