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
        public void CalculateDifferenceHash64()
        {
            var hash = _imgHashes.CalculateDifferenceHash64(@"");

            Assert.Equal(0UL, hash);
        }

        [Fact]
        public void CalculateMedianHash64()
        {
            var hash = _imgHashes.CalculateMedianHash64(@"");

            Assert.Equal(0xffffffff00000000, hash);
        }

        [Fact]
        public void CalculateMedianHash256()
        {
            var hash = _imgHashes.CalculateMedianHash256(@"");

            Assert.Equal(0x0000000000000000UL, hash[0]);
            Assert.Equal(0x0000000000000000UL, hash[1]);
            Assert.Equal(0xffffffffffffffffUL, hash[2]);
            Assert.Equal(0xffffffffffffffffUL, hash[3]);
        }

        [Fact]
        public void CalculateDctHash64()
        {
            var hash = _imgHashes.CalculateDctHash("");

            Assert.Equal(0xa4f8d63986aa52ad, hash);
        }

        [Fact]
        public void CompareHashes_notEqualLength()
        {
            var hash1 = new ulong[1];
            var hash2 = new ulong[2];

            var exception = Record.Exception(() => ImageHashes.CompareHashes(hash1, hash2));
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void CompareHashes_identicalHashesSize64()
        {
            var hash1 = new ulong[1];
            var hash2 = new ulong[1];

            hash1[0] = 0x0fff0000ffff0000;
            hash2[0] = 0x0fff0000ffff0000;

            var result = ImageHashes.CompareHashes(hash1, hash2);
            Assert.Equal(1.0f, result, 4);
        }

        [Fact]
        public void CompareHashes_identicalHashesSize256()
        {
            var hash1 = new ulong[4];
            var hash2 = new ulong[4];

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
            var hash1 = new ulong[1];
            var hash2 = new ulong[1];

            hash1[0] = 0L;
            hash2[0] = ulong.MaxValue;

            var result = ImageHashes.CompareHashes(hash1, hash2);
            Assert.Equal(0.0f, result, 4);
        }

        [Fact]
        public void CompareHashes_halfIdenticalHashes()
        {
            var hash1 = new ulong[1];
            var hash2 = new ulong[1];

            hash1[0] = 0x00000000ffffffff;
            hash2[0] = ulong.MaxValue;

            var result = ImageHashes.CompareHashes(hash1, hash2);
            Assert.Equal(0.5f, result, 4);
        }
    }
}
