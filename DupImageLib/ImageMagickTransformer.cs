using ImageMagick;

namespace DupImageLib
{
    public class ImageMagickTransformer : IImageTransformer
    {
        public byte[] TransformImage(string path, int width, int height)
        {
            // Read image
            var img = new MagickImage(path);

            var settings = new QuantizeSettings
            {
                ColorSpace = ColorSpace.Gray,
                Colors = 256
            };

            img.Quantize(settings);

            var size = new MagickGeometry(width, height) { IgnoreAspectRatio = true };

            img.Resize(size);
            
            // Get pixel data from image and only return single channel instead of rgb data.
            var imgPixels = img.GetPixels().GetValues();
            var pixels = new byte[width * height];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = imgPixels[i * 3];
            }

            return pixels;
        }
    }
}