namespace DupImage
{
    public static class ImageHashes
    {
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
            var hashSize = image1.Hash.Count;
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
