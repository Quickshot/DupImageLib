using System;
using System.Collections.Generic;
using System.IO;

namespace DupImage
{
    /// <summary>
    /// Structure for containing image information and hash values.
    /// </summary>
    public class ImageStruct
    {
        /// <summary>
        /// Construct a new ImageStruct from FileInfo.
        /// </summary>
        /// <param name="file">FileInfo to be used.</param>
        public ImageStruct(FileInfo file)
        {
            File = file;

            // Init Hash
            Hash = new List<long>();
        }

        /// <summary>
        /// Construct a new ImageStruct from image path.
        /// </summary>
        /// <param name="pathToImage">Image location</param>
        public ImageStruct(String pathToImage)
        {
            File = new FileInfo(pathToImage);

            // Init Hash
            Hash = new List<long>();
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
