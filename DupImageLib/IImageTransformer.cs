namespace DupImageLib
{
    public interface IImageTransformer
    {
        byte[] TransformImage(string path, int width, int height);
    }
}