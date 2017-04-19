using System;
using Xunit;

namespace DupImageLib.Tests
{
    public class ImageHashesTests
    {
        private readonly ImageHashes _imgHashes;

        public ImageHashesTests()
        {
            _imgHashes = new ImageHashes(new DummyImageTransformer());
        }

        [Fact]
        public void Test1()
        {
            var hash = _imgHashes.CalculateMedianHash64(@"C:\Users\jptei\Dropbox\Pictures\Wallpapers Additional\00a10f6575cf41bce326f03bf5482fc2440727c4.jpg");

            Assert.NotEqual(0L, hash);
        }

        [Fact]
        public void CompareHashes_notEqualLength()
        {
            var hash1 = new long[1];
            var hash2 = new long[2];

            var exception = Record.Exception(() => ImageHashes.CompareHashes(hash1, hash2));
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void CompareHashes_identicalHashesSize64()
        {
            var hash1 = new long[1];
            var hash2 = new long[1];

            hash1[0] = 0x0fff0000ffff0000;
            hash2[0] = 0x0fff0000ffff0000;

            var result = ImageHashes.CompareHashes(hash1, hash2);
            Assert.Equal(1.0f, result, 4);
        }

        [Fact]
        public void CompareHashes_identicalHashesSize256()
        {
            var hash1 = new long[4];
            var hash2 = new long[4];

            hash1[0] = 0x0fff0000ffff0000;
            hash1[1] = 0x0fff0000ffff0000;
            hash1[2] = 0x0fff0000ffff0000;
            hash1[3] = 0x0fff0000ffff0000;

            hash2[0] = 0x0fff0000ffff0000;
            hash2[1] = 0x0fff0000ffff0000;
            hash2[2] = 0x0fff0000ffff0000;
            hash2[3] = 0x0fff0000ffff0000;

            var result = ImageHashes.CompareHashes(hash1, hash2);
            Assert.Equal(1.0f, result, 4);
        }

        [Fact]
        public void CompareHashes_nonIdenticalHashes()
        {
            var hash1 = new long[1];
            var hash2 = new long[1];

            hash1[0] = 0L;
            hash2[0] = -1L;

            var result = ImageHashes.CompareHashes(hash1, hash2);
            Assert.Equal(0.0f, result, 4);
        }

        [Fact]
        public void CompareHashes_halfIdenticalHashes()
        {
            var hash1 = new long[1];
            var hash2 = new long[1];

            hash1[0] = 0x00000000ffffffff;
            hash2[0] = -1L;

            var result = ImageHashes.CompareHashes(hash1, hash2);
            Assert.Equal(0.5f, result, 4);
        }
    }
}
