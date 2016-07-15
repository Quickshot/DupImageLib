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
            if (file == null) throw new ArgumentNullException(nameof(file));

            ImagePath = file.FullName;

            // Init Hash
            Hash = new long[1];
        }

        /// <summary>
        /// Construct a new ImageStruct from image path.
        /// </summary>
        /// <param name="pathToImage">Image location</param>
        public ImageStruct(String pathToImage)
        {
            ImagePath = pathToImage;

            // Init Hash
            Hash = new long[1];
        }

        /// <summary>
        /// ImagePath information.
        /// </summary>
        public String ImagePath { get; private set; }

        /// <summary>
        /// Hash of the image. Uses longs instead of ulong to be CLS compliant.
        /// </summary>
        public long[] Hash { get; set; }

    }
}
