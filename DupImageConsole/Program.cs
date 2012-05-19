using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DupImage;

namespace DupImageConsole
{
    static class Program
    {
        static void Main(string[] args)
        {
            // Path to image
            var hash = ImageHashes.CalculateMedianHash64(@"D:\SkyDrive\Pictures\moominhaddockhuge.png");
            Console.WriteLine("Hash of the image is: {0:X}", hash);
            var hash2 = ImageHashes.CalculateMedianHash256(@"D:\SkyDrive\Pictures\moominhaddockhuge.png");
            Console.WriteLine("Hash of the image is: {0:X}{1:X}{2:X}{3:X}", hash2[0], hash2[1], hash2[2], hash2[3]);
        }
    }
}
