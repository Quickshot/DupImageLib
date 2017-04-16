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
            image.CacheOption = BitmapCacheOption.OnLoad;
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
        public static void CalculateMedianHash(ImageStruct image, bool useLargeHash)
        {
            // Null check
            if (image == null) throw new ArgumentNullException(nameof(image));

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
        /// Calculates 64 bit hash for the given image using difference hash.
        /// 
        /// See http://www.hackerfactor.com/blog/index.php?/archives/529-Kind-of-Like-That.html for algorithm description.
        /// </summary>
        /// <param name="pathToImage">Path to image being hashed</param>
        /// <returns>64 bit difference hash</returns>
        public static long CalculateDifferenceHash64(string pathToImage)
        {
            // Read image and resize. Ignores color profile for increased performance.
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(pathToImage, UriKind.RelativeOrAbsolute);
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.DecodePixelHeight = 8;
            image.DecodePixelWidth = 9;
            image.EndInit();

            // Convert to grayscale
            var grayImage = new FormatConvertedBitmap();
            grayImage.BeginInit();
            grayImage.Source = image;
            grayImage.DestinationFormat = System.Windows.Media.PixelFormats.Gray8;
            grayImage.EndInit();

            // Copy pixel data
            var pixels = new byte[grayImage.PixelHeight * 9]; // Must be a multiple of stride for CopyPixel to work
            grayImage.CopyPixels(pixels, 9, 0);

            // Iterate pixels and set hash to 1 if the left pixel is brighter than the right pixel.
            var hash = 0UL;
            var hashPos = 0;
            for (var i = 0; i < 8; i++)
            {
                var rowStart = i * 9;
                for (var j = 0; j < 8; j++)
                {
                    if (pixels[rowStart + j] > pixels[rowStart + j + 1])
                    {
                        hash |= (1UL << hashPos);
                    }
                    hashPos++;
                }
            }

            // Done
            return (long)hash;
        }

        /// <summary>
        /// Calculates 64 bit hash for the given ImageStruct using difference hash.
        /// </summary>
        /// <param name="image">ImageStruct used for hash calculation.</param>
        public static void CalculateDifferenceHash(ImageStruct image)
        {
            // Null check
            if (image == null) throw new ArgumentNullException(nameof(image));

            image.Hash = new long[1];
            image.Hash[0] = CalculateDifferenceHash64(image.ImagePath);
        }

        /// <summary>
        /// Calculates a hash for the given ImageStruct using dct algorithm
        /// </summary>
        /// <param name="image">ImageStruct used for hash calculation.</param>
        /// <param name="dctMatrix">DCT coefficient matrix to be used.</param>
        public static void CalculateDctHash(ImageStruct image, float[][] dctMatrix)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            image.Hash = new long[1];
            image.Hash[0] = CalculateDctHash(image.ImagePath, dctMatrix);
        }

        /// <summary>
        /// Calculates a hash for the given image using dct algorithm
        /// </summary>
        /// <param name="path">Path for the image used for hash calculation.</param>
        /// <param name="dctMatrix">DCT coefficient matrix to be used.</param>
        /// <returns>64bit hash of the image</returns>
        public static long CalculateDctHash(string path, float[][] dctMatrix)
        {
            // Read image and resize. Ignores color profile for increased performance.
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.CacheOption = BitmapCacheOption.OnLoad;
            // 32*32 image for dct
            image.DecodePixelHeight = 32;
            image.DecodePixelWidth = 32;
            image.EndInit();

            // Convert to grayscale
            var grayImage = new FormatConvertedBitmap();
            grayImage.BeginInit();
            grayImage.Source = image;
            grayImage.DestinationFormat = System.Windows.Media.PixelFormats.Gray8;
            grayImage.EndInit();

            // Copy pixel data
            var pixels = new byte[1024];
            grayImage.CopyPixels(pixels, 32, 0);

            // Convert to float
            var fPixels = new float[1024];
            for (var i = 0; i < 1024; i++)
            {
                fPixels[i] = pixels[i]/255.0f;
            }

            // Calculate dct
            var dctPixels = ComputeDct(fPixels, dctMatrix);

            // Get 8*8 area from 1,1 to 8,8, ignoring lowest frequencies for improved detection
            var dctHashPixels = new float[64];
            for (var x = 0; x < 8; x++)
            {
                for (var y = 0; y < 8; y++)
                {
                    dctHashPixels[x + y*8] = dctPixels[x+1][y+1];
                }
            }

            // Calculate median
            var pixelList = new List<float>(dctHashPixels);
            pixelList.Sort();
            // Even amount of pixels
            var median = (pixelList[31] + pixelList[32]) / 2;

            // Iterate pixels and set them to 1 if over median and 0 if lower.
            var hash = 0UL;
            for (var i = 0; i < 64; i++)
            {
                if (dctHashPixels[i] > median)
                {
                    hash |= (1UL << i);
                }
            }

            // Done
            return (long)hash;
        }

        /// <summary>
        /// Calculates a hash for the given image using dct algorithm
        /// </summary>
        /// <param name="path">Path for the image used for hash calculation.</param>
        /// <returns>64bit hash of the image</returns>
        public static long CalculateDctHash(string path)
        {
            var dctCoef = GenerateDctMatrix(32);
            return CalculateDctHash(path, dctCoef);
        }

        /// <summary>
        /// Compute DCT for the image.
        /// </summary>
        /// <param name="image">Image to calculate the dct.</param>
        /// <param name="dctMatrix">DCT coefficient matrix</param>
        /// <returns>DCT transform of the image</returns>
        private static float[][] ComputeDct(float[] image, float[][] dctMatrix)
        {
            // Get the size of dct matrix. We assume that the image is same size as dctMatrix
            var size = dctMatrix.GetLength(0);
            
            // Make image matrix
            var imageMat = new float[size][];
            for (var i = 0; i < size; i++)
            {
                imageMat[i] = new float[size];
            }

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    imageMat[y][x] = image[x + y*size];
                }
            }

            return Multiply(Multiply(dctMatrix, imageMat), Transpose(dctMatrix));
        }

        /// <summary>
        /// Generates DCT coefficient matrix.
        /// </summary>
        /// <param name="size">Size of the matrix.</param>
        /// <returns>Coefficient matrix.</returns>
        public static float[][] GenerateDctMatrix(int size)
        {
            var matrix = new float[size][];
            for (int i = 0; i < size; i++)
            {
                matrix[i] = new float[size];
            }

            var c1 = Math.Sqrt(2.0f/size);

            for (var j = 0; j < size; j++)
            {
                matrix[0][j] = (float)Math.Sqrt(1.0d / size);
            }

            for (var j = 0; j < size; j++)
            {
                for (var i = 1; i < size; i++)
                {
                    matrix[i][j] = (float) (c1*Math.Cos(((2 * j + 1) * i * Math.PI) / (2.0d * size)));
                }
            }
            return matrix;
        }

        /// <summary>
        /// Matrix multiplication.
        /// </summary>
        /// <param name="a">First matrix.</param>
        /// <param name="b">Second matric.</param>
        /// <returns>Result matrix.</returns>
        private static float[][] Multiply(float[][] a, float[][] b)
        {
            var n = a[0].Length;
            var c = new float[n][];
            for (var i = 0; i < n; i++)
            {
                c[i] = new float[n];
            }

            for (var i = 0; i < n; i++)
                for (var k = 0; k < n; k++)
                    for (var j = 0; j < n; j++)
                        c[i][j] += a[i][k] * b[k][j];
            return c;
        }

        /// <summary>
        /// Transposes square matrix.
        /// </summary>
        /// <param name="mat">Matrix to be transposed</param>
        /// <returns>Transposed matrix</returns>
        private static float[][] Transpose(float[][] mat)
        {
            var size = mat[0].Length;
            var transpose = new float[size][];

            for (var i = 0; i < size; i++)
            {
                transpose[i] = new float[size];
                for (int j = 0; j < size; j++)
                    transpose[i][j] = mat[j][i];
            }
            return transpose;
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
                throw new ArgumentNullException(nameof(image1));
            }
            if (image2 == null)
            {
                throw new ArgumentNullException(nameof(image2));
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
            return 1.0f - onesInHash/(hashSize * 64.0f);    //Assuming 64bit variables
        }

        // Hamming distance constants. See http://en.wikipedia.org/wiki/Hamming_weight for explanation.
        private const ulong M1 = 0x5555555555555555; //binary: 0101...
        private const ulong M2 = 0x3333333333333333; //binary: 00110011..
        private const ulong M4 = 0x0f0f0f0f0f0f0f0f; //binary:  4 zeros,  4 ones ...
        private const ulong H01 = 0x0101010101010101; //the sum of 256 to the power of 0,1,2,3...
    }
}
