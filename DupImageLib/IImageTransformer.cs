using System.IO;

namespace DupImageLib
{
    public interface IImageTransformer
    {
        byte[] TransformImage(Stream stream, int width, int height);
    }
}