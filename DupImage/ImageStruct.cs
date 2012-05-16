using System;
using System.Collections.Generic;
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
            : this()
        {
            File = file;

            // Reserve space for 256 bit hash.
            Hash = new List<long>(4);
        }

        /// <summary>
        /// Construct a new ImageStruct from image path.
        /// </summary>
        /// <param name="pathToImage">Image location</param>
        public ImageStruct(String pathToImage) : this()
        {
            File = new FileInfo(pathToImage);

            // Reserve space for 256 bit hash.
            Hash = new List<long>(4);
        }

        /// <summary>
        /// File information.
        /// </summary>
        public FileInfo File { get; set; }

        /// <summary>
        /// Hash of the image. Uses longs instead of ulong to be CLS compliant.
        /// </summary>
        public List<long> Hash { get; set; }
    }
}
