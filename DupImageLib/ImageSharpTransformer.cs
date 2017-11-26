using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;

namespace DupImageLib
{
    public class ImageSharpTransformer : IImageTransformer
    {
        public byte[] TransformImage(Stream stream, int width, int height)
        {
            byte[] imageBytes;
            using (Image<Rgba32> image = Image.Load(stream))
            {
                image.Mutate(x => x
                    .Resize(width, height)
                    .Grayscale());
                imageBytes = image.SavePixelData();
            }
            byte[] bytes = new byte[imageBytes.Length/4];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = imageBytes[i * 4];
            }

            return bytes;
        }
    }
}
