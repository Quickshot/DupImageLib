using System;
using System.Collections.Generic;
using System.IO;

namespace DupImageLib
{
    public class ImageHashes
    {
        private readonly IImageTransformer _transformer;

        private float[][] _dctMatrix;
        private bool _isDctMatrixInitialized = false;
        private readonly object _dctMatrixLockObject = new object();

        /// <summary>
        /// Initializes a new instance of the ImageHashes class using ImageMagickTransformer.
        /// </summary>
        public ImageHashes()
        {
            _transformer = new ImageMagickTransformer();
        }

        /// <summary>
        /// Initializes a new instance of the ImageHashes class using the given IImageTransformer.
        /// </summary>
        /// <param name="transformer">Implementation of the IImageTransformer to be used for image transformation.</param>
        public ImageHashes(IImageTransformer transformer)
        {
            _transformer = transformer;
        }

        /// <summary>
        /// Calculates a 64 bit hash for the given image using median algorithm.
        /// 
        /// Works by converting the image to 8x8 greyscale image, finding the median pixel value from it, and then marking
        /// all pixels where value is greater than median value as 1 in the resulting hash. Should be more resistant to non-linear
        /// image edits when compared agains average based implementation.
        /// </summary>
        /// <param name="pathToImage">Path to an image to be hashed.</param>
        /// <returns>64 bit median hash of the input image.</returns>
        public ulong CalculateMedianHash64(string pathToImage)
        {
            var stream = new FileStream(pathToImage, FileMode.Open);
            return CalculateMedianHash64(stream);
        }

        /// <summary>
        /// Calculates a 64 bit hash for the given image using median algorithm.
        /// 
        /// Works by converting the image to 8x8 greyscale image, finding the median pixel value from it, and then marking
        /// all pixels where value is greater than median value as 1 in the resulting hash. Should be more resistant to non-linear
        /// image edits when compared agains average based implementation.
        /// </summary>
        /// <param name="sourceStream">Stream containing an image to be hashed.</param>
        /// <returns>64 bit median hash of the input image.</returns>
        public ulong CalculateMedianHash64(Stream sourceStream)
        {
            var pixels = _transformer.TransformImage(sourceStream, 8, 8);

            // Calculate median
            var pixelList = new List<byte>(pixels);
            pixelList.Sort();
            // Even amount of pixels
            var median = (byte)((pixelList[31] + pixelList[32]) / 2);

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
            return hash;
        }

        /// <summary>
        /// Calculates a 256 bit hash for the given image using median algorithm.
        /// 
        /// Works by converting the image to 16x16 greyscale image, finding the median pixel value from it, and then marking
        /// all pixels where value is greater than median value as 1 in the resulting hash. Should be more resistant to non-linear
        /// image edits when compared agains average based implementation.
        /// </summary>
        /// <param name="pathToImage">Path to an image to be hashed.</param>
        /// <returns>256 bit median hash of the input image. Composed of 4 ulongs.</returns>
        public ulong[] CalculateMedianHash256(string pathToImage)
        {
            var stream = new FileStream(pathToImage, FileMode.Open);
            return CalculateMedianHash256(stream);
        }

        /// <summary>
        /// Calculates a 256 bit hash for the given image using median algorithm.
        /// 
        /// Works by converting the image to 16x16 greyscale image, finding the median pixel value from it, and then marking
        /// all pixels where value is greater than median value as 1 in the resulting hash. Should be more resistant to non-linear
        /// image edits when compared agains average based implementation.
        /// </summary>
        /// <param name="sourceStream">Stream containing an image to be hashed.</param>
        /// <returns>256 bit median hash of the input image. Composed of 4 ulongs.</returns>
        public ulong[] CalculateMedianHash256(Stream sourceStream)
        {
            var pixels = _transformer.TransformImage(sourceStream, 16, 16);

            // Calculate median
            var pixelList = new List<byte>(pixels);
            pixelList.Sort();
            // Even amount of pixels
            var median = (byte)((pixelList[127] + pixelList[128]) / 2);

            // Iterate pixels and set them to 1 if over median and 0 if lower.
            var hash64 = 0UL;
            var hash = new ulong[4];
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 64; j++)
                {
                    if (pixels[64 * i + j] > median)
                    {
                        hash64 |= (1UL << j);
                    }
                }
                hash[i] = hash64;
                hash64 = 0UL;
            }

            // Done
            return hash;
        }

        /// <summary>
        /// Calculates 64 bit hash for the given image using difference hash.
        /// 
        /// See http://www.hackerfactor.com/blog/index.php?/archives/529-Kind-of-Like-That.html for algorithm description.
        /// </summary>
        /// <param name="pathToImage">Path to an image to be hashed.</param>
        /// <returns>64 bit difference hash of the input image.</returns>
        public ulong CalculateDifferenceHash64(string pathToImage)
        {
            var stream = new FileStream(pathToImage, FileMode.Open);
            return CalculateDifferenceHash64(stream);
        }

        /// <summary>
        /// Calculates 64 bit hash for the given image using difference hash.
        /// 
        /// See http://www.hackerfactor.com/blog/index.php?/archives/529-Kind-of-Like-That.html for algorithm description.
        /// </summary>
        /// <param name="sourceStream">Stream containing an image to be hashed.</param>
        /// <returns>64 bit difference hash of the input image.</returns>
        public ulong CalculateDifferenceHash64(Stream sourceStream)
        {
            var pixels = _transformer.TransformImage(sourceStream, 9, 8);

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
            return hash;
        }

        /// <summary>
        /// Calculates 256 bit hash for the given image using difference hash.
        /// 
        /// See http://www.hackerfactor.com/blog/index.php?/archives/529-Kind-of-Like-That.html for algorithm description.
        /// </summary>
        /// <param name="pathToImage">Path to an image to be hashed.</param>
        /// <returns>64 bit difference hash of the input image.</returns>
        public ulong[] CalculateDifferenceHash256(string pathToImage)
        {
            var stream = new FileStream(pathToImage, FileMode.Open);
            return CalculateDifferenceHash256(stream);
        }

        /// <summary>
        /// Calculates 256 bit hash for the given image using difference hash.
        /// 
        /// See http://www.hackerfactor.com/blog/index.php?/archives/529-Kind-of-Like-That.html for algorithm description.
        /// </summary>
        /// <param name="sourceStream">Stream containing an image to be hashed.</param>
        /// <returns>64 bit difference hash of the input image.</returns>
        public ulong[] CalculateDifferenceHash256(Stream sourceStream)
        {
            var pixels = _transformer.TransformImage(sourceStream, 17, 16);

            // Iterate pixels and set hash to 1 if the left pixel is brighter than the right pixel.
            var hash = new ulong[4];
            var hashPos = 0;
            var hashPart = 0;
            for (var i = 0; i < 16; i++)
            {
                var rowStart = i * 17;
                for (var j = 0; j < 16; j++)
                {
                    if (pixels[rowStart + j] > pixels[rowStart + j + 1])
                    {
                        hash[hashPart] |= (1UL << hashPos);
                    }
                    if (hashPos == 63)
                    {
                        hashPos = 0;
                        hashPart++;
                    }
                    else
                    {
                        hashPos++;
                    }
                }
            }

            // Done
            return hash;
        }

        /// <summary>
        /// Calculates a hash for the given image using dct algorithm
        /// </summary>
        /// <param name="sourceStream">Stream to the image used for hash calculation.</param>
        /// <returns>64 bit difference hash of the input image.</returns>
        public ulong CalculateDctHash(Stream sourceStream)
        {
            lock (_dctMatrixLockObject)
            {
                if (!_isDctMatrixInitialized)
                {
                    _dctMatrix = GenerateDctMatrix(32);
                    _isDctMatrixInitialized = true;
                }
            }

            var pixels = _transformer.TransformImage(sourceStream, 32, 32);

            // Copy pixel data and convert to float
            var fPixels = new float[1024];
            for (var i = 0; i < 1024; i++)
            {
                fPixels[i] = pixels[i] / 255.0f;
            }

            // Calculate dct
            var dctPixels = ComputeDct(fPixels, _dctMatrix);

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
            return hash;
        }

        /// <summary>
        /// Calculates a hash for the given image using dct algorithm
        /// </summary>
        /// <param name="path">Path to the image used for hash calculation.</param>
        /// <returns>64 bit difference hash of the input image.</returns>
        public ulong CalculateDctHash(string path)
        {
            var stream = new FileStream(path, FileMode.Open);
            return CalculateDctHash(stream);
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
        private static float[][] GenerateDctMatrix(int size)
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
                for (var j = 0; j < size; j++)
                    transpose[i][j] = mat[j][i];
            }
            return transpose;
        }

        /// <summary>
        /// Compare hashes of two images using Hamming distance. Result of 1 indicates images being 
        /// same, while result of 0 indicates completely different images.
        /// </summary>
        /// <param name="hash1">First hash to be compared</param>
        /// <param name="hash2">Second hash to be compared</param>
        /// <returns>Image similarity in range [0,1]</returns>
        public static float CompareHashes(ulong hash1, ulong hash2)
        {
            // XOR hashes
            var hashDifference = hash1 ^ hash2;

            // Calculate ones
            var onesInHash = HammingWeight(hashDifference);

            // Return result as a float between 0 and 1.
            return 1.0f - onesInHash / 64.0f;
        }

        /// <summary>
        /// Compare hashes of two images using Hamming distance. Result of 1 indicates images being 
        /// same, while result of 0 indicates completely different images. Hash size is inferred from 
        /// the size of Hash array in first image.
        /// </summary>
        /// <param name="hash1">First hash to be compared</param>
        /// <param name="hash2">Second hash to be compared</param>
        /// <returns>Image similarity in range [0,1]</returns>
        public static float CompareHashes(ulong[] hash1, ulong[] hash2)
        {
            // Check that hash lengths are same
            if (hash1.Length != hash2.Length)
            {
                throw new ArgumentException("Lengths of hash1 and hash2 do not match.");
            }

            var hashSize = hash1.Length;
            ulong onesInHash = 0;

            // XOR hashes
            var hashDifference = new ulong[hashSize];
            for (var i = 0; i < hashSize; i++)  // Slightly faster than foreach
            {
                hashDifference[i] = hash1[i] ^ hash2[i];
            }

            // Calculate ones
            for (var i = 0; i < hashSize; i++)
            {
                onesInHash += HammingWeight(hashDifference[i]);
            }

            // Return result as a float between 0 and 1.
            return 1.0f - onesInHash/(hashSize * 64.0f);    //Assuming 64bit variables
        }

        /// <summary>
        /// Calculate ones in hash using Hamming weight. See http://en.wikipedia.org/wiki/Hamming_weight
        /// </summary>
        /// <param name="hash">Input value</param>
        /// <returns>Count of ones in input value</returns>
        private static ulong HammingWeight(ulong hash)
        {
            var onesInHash = 0UL;
            
            hash -= (hash >> 1) & M1;
            hash = (hash & M2) + ((hash >> 2) & M2);
            hash = (hash + (hash >> 4)) & M4;
            onesInHash = (hash * H01) >> 56;

            return onesInHash;
        }

        // Hamming distance constants. See http://en.wikipedia.org/wiki/Hamming_weight for explanation.
        private const ulong M1 = 0x5555555555555555; //binary: 0101...
        private const ulong M2 = 0x3333333333333333; //binary: 00110011..
        private const ulong M4 = 0x0f0f0f0f0f0f0f0f; //binary:  4 zeros,  4 ones ...
        private const ulong H01 = 0x0101010101010101; //the sum of 256 to the power of 0,1,2,3...
    }
}
