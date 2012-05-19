using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace DupImage
{
    public static class ImageHashes
    {
        /// <summary>
        /// Calculates a 64 bit hash for the given image using median algorithm.
        /// </summary>
        /// <param name="pathToImage">Path to image being hashed</param>
        /// <returns>64 bit median hash</returns>
        public static long CalculateMedianHash64(string pathToImage)
        {
            // Read image and resize. Ignores color profile for increased performance.
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(pathToImage, UriKind.RelativeOrAbsolute);
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.DecodePixelHeight = 8;
            image.DecodePixelWidth = 8;
            image.EndInit();

            // Convert to grayscale
            var grayImage = new FormatConvertedBitmap();
            grayImage.BeginInit();
            grayImage.Source = image;
            grayImage.DestinationFormat = System.Windows.Media.PixelFormats.Gray8;
            grayImage.EndInit();

            // Copy pixel data
            var pixels = new byte[64];
            grayImage.CopyPixels(pixels, 8, 0);

            // Calculate median
            var pixelList = new List<byte>(pixels);
            pixelList.Sort();
            // Even amount of pixels
            var median = (byte) ((pixelList[31] + pixelList[32])/2);

            // Iterate pixels and set them to 1 if over median and 0 if lower.
            var hash = 0UL;
            for (var i = 0; i < 64; i++)
            {
                if (pixels[i] > median)
                {
                    hash |= (1UL << i);
                }
            }

            // Done
            return (long)hash;
        }

        /// <summary>
        /// Calculates a 256 bit hash for the given image using median algorithm.
        /// </summary>
        /// <param name="pathToImage">Path to image being hashed.</param>
        /// <returns>256 bit median hash. Composed of 4 longs.</returns>
        public static long[] CalculateMedianHash256(string pathToImage)
        {
            // Read image and resize. Ignores color profile for increased performance.
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(pathToImage, UriKind.RelativeOrAbsolute);
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            // 16*16 image for 256 bit hash
            image.DecodePixelHeight = 16;
            image.DecodePixelWidth = 16;
            image.EndInit();

            // Convert to grayscale
            var grayImage = new FormatConvertedBitmap();
            grayImage.BeginInit();
            grayImage.Source = image;
            grayImage.DestinationFormat = System.Windows.Media.PixelFormats.Gray8;
            grayImage.EndInit();

            // Copy pixel data
            var pixels = new byte[256];
            grayImage.CopyPixels(pixels, 16, 0);

            // Calculate median
            var pixelList = new List<byte>(pixels);
            pixelList.Sort();
            // Even amount of pixels
            var median = (byte)((pixelList[63] + pixelList[64]) / 2);

            // Iterate pixels and set them to 1 if over median and 0 if lower.
            var hash64 = 0UL;
            var hash = new long[4];
            for (var i = 0; i < 4; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    if (pixels[64*i + j] > median)
                    {
                        hash64 |= (1UL << j);
                    }
                }
                hash[i] = (long)hash64;
                hash64 = 0UL;
            }

            // Done
            return hash;
        }

        /// <summary>
        /// Calculates a hash for the given ImageStruct using median algorithm. Hash size can be either 64 bits or 256 bits.
        /// </summary>
        /// <param name="image">ImageStruct used for hash calculation.</param>
        /// <param name="useLargeHash">Indicates whether to calculate 256 bit hash or not. True for 256 bit hash.</param>
        public static void CalculateMedianHash(ImageStruct image, bool useLargeHash = false)
        {
            // Null check
            if (image == null) throw new ArgumentNullException("image");

            if (useLargeHash)
            {
                image.Hash = CalculateMedianHash256(image.ImagePath);
            }
            else
            {
                image.Hash = new long[1];
                image.Hash[0] = CalculateMedianHash64(image.ImagePath);
            }
        }

        /// <summary>
        /// Compare hashes of two images using Hamming distance. Result of 1 indicates images being 
        /// same, while result of 0 indicates completely different images. Hash size is inferred from 
        /// the size of Hash array in first image.
        /// </summary>
        /// <param name="image1">First image to be compared</param>
        /// <param name="image2">Second image to be compared</param>
        /// <returns>Image similarity in range [0,1]</returns>
        public static float CompareHashes(ImageStruct image1, ImageStruct image2)
        {
            // Chack for null references. Throw exception in case of null
            if (image1 == null)
            {
                throw new ArgumentNullException("image1");
            }
            if (image2 == null)
            {
                throw new ArgumentNullException("image2");
            }

            var hashSize = image1.Hash.Length;
            ulong onesInHash = 0;

            // XOR hashes
            var hashDifference = new ulong[hashSize];
            for (var i = 0; i < hashSize; i++)  // Slightly faster than foreach
            {
                hashDifference[i] = (ulong)image1.Hash[i] ^ (ulong)image2.Hash[i];
            }

            // Calculate ones using Hamming weight. See http://en.wikipedia.org/wiki/Hamming_weight
            for (var i = 0; i < hashSize; i++)
            {
                var x = hashDifference[i];
                x -= (x >> 1) & M1;
                x = (x & M2) + ((x >> 2) & M2);
                x = (x + (x >> 4)) & M4;
                onesInHash += (x * H01) >> 56;
            }

            // Return result as a float between 0 and 1.
            return onesInHash/(hashSize * 64.0f);    //Assuming 64bit variables
        }

        // Hamming distance constants. See http://en.wikipedia.org/wiki/Hamming_weight for explanation.
        private const ulong M1 = 0x5555555555555555; //binary: 0101...
        private const ulong M2 = 0x3333333333333333; //binary: 00110011..
        private const ulong M4 = 0x0f0f0f0f0f0f0f0f; //binary:  4 zeros,  4 ones ...
        private const ulong H01 = 0x0101010101010101; //the sum of 256 to the power of 0,1,2,3...
    }
}
