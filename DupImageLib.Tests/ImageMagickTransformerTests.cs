using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DupImageLib.Tests
{
    public class ImageMagickTransformerTests
    {
        private readonly ImageHashes _imgHashes;

        public ImageMagickTransformerTests()
        {
            _imgHashes = new ImageHashes(new ImageSharpTransformer());
        }

        [Fact]
        public void ComputeHash()
        {
            var hash = _imgHashes.CalculateMedianHash64("testPattern1.png");
            Assert.Equal(0x0f0f0f0f0f0f0f0fUL, hash);
        }

        [Fact]
        public void CompareHashes_Size64()
        {
            var hash1 = _imgHashes.CalculateMedianHash64("testPattern1.png");
            var hash2 = _imgHashes.CalculateMedianHash64("testPattern2.png");

            var differrence = ImageHashes.CompareHashes(hash1, hash2);

            Assert.InRange(differrence, 0.49, 0.51);
        }
    }
}
