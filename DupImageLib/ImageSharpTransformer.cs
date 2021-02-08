using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace DupImageLib
{
    /// <summary>
    /// Implements IImageTransformer interface using ImageSharp for image transforms.
    /// </summary>
    public class ImageSharpTransformer : IImageTransformer
    {
        public byte[] TransformImage(Stream stream, int width, int height)
        {
            using var image = Image.Load<Rgba32>(stream);
            
            image.Mutate(x => x.Resize(new ResizeOptions()
            {
                Size = new Size()
                {
                    Width = width,
                    Height = height
                },
                Mode = ResizeMode.Stretch,
                Sampler = new BicubicResampler()
            }).Grayscale());

            image.TryGetSinglePixelSpan(out var pixelSpan);
            var pixelArray = pixelSpan.ToArray();

            var pixelCount = width * height;
            var bytes = new byte[pixelCount];
            for (var i = 0; i < pixelCount; i++)
            {
                bytes[i] = pixelArray[i].B;
            }

            return bytes;
        }
    }
}