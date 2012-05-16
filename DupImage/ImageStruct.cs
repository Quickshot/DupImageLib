using System;
using System.IO;

namespace DupImage
{
    /// <summary>
    /// Structure for containing image information and hash values.
    /// </summary>
    public struct ImageStruct
    {
        /// <summary>
        /// Construct a new ImageStruct from FileInfo.
        /// </summary>
        /// <param name="file">FileInfo to be used.</param>
        public ImageStruct(FileInfo file)
        {
            File = file;

            // Reserve space for 256 bit hash.
            Hash = new ulong[4];
        }

        /// <summary>
        /// Construct a new ImageStruct from image path.
        /// </summary>
        /// <param name="pathToImage">Image location</param>
        public ImageStruct(String pathToImage)
        {
            File = new FileInfo(pathToImage);

            // Reserve space for 256 bit hash.
            Hash = new ulong[4];
        }

        /// <summary>
        /// File information.
        /// </summary>
        public FileInfo File;

        /// <summary>
        /// Hash of the image.
        /// </summary>
        public ulong[] Hash;

    }
}
