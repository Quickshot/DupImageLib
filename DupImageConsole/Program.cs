using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;
using CommandLine.Text;
using DupImage;

namespace DupImageConsole
{
    static class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            var parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));

            if (parser.ParseArguments(args, options))
            {
                // Parsed arguments, so lets get to work.
                var dirInfo = new DirectoryInfo(options.Directory);
                FullDirList(dirInfo, "*.*", options.Recursive);
                // Filter files
                var searchPattern = new Regex(@"\.(jpg|jpeg|png|tif|tiff|gif|bmp|wmp)$", RegexOptions.IgnoreCase);

                var filteredFiles = files.Where(f => searchPattern.IsMatch(f.ImagePath)).ToList();
                
                // Lets try to calculate hashes
                var images = new List<ImageStruct>();
                Console.WriteLine("Calculating hashes...");
                for (var i = 0; i < filteredFiles.Count; i++)
                {
                    try
                    {
                        if (options.LargeHash)
                        {
                            ImageHashes.CalculateMedianHash(filteredFiles[i], true);
                            images.Add(filteredFiles[i]);
                        }
                        else
                        {
                            ImageHashes.CalculateMedianHash(filteredFiles[i]);
                            images.Add(filteredFiles[i]);
                        }
                    }
                    catch (Exception)
                    {
                        // Meh
                    }
                }
                // Hash comparison
                Console.WriteLine("Comparing hashes...");
                for (int i = 0; i < images.Count; i++)
                {
                    for (int j = i+1; j < images.Count; j++)
                    {
                        var similarity = ImageHashes.CompareHashes(images[i], images[j]);
                        if (similarity > 0.5f)
                        {
                            Console.WriteLine("Match found: Similarity {0}", similarity);
                            Console.WriteLine("{0}", images[i].ImagePath);
                            Console.WriteLine("{0}", images[j].ImagePath);
                        }
                    }
                }
            }
        }

        private static List<ImageStruct> files = new List<ImageStruct>();  // List that will hold the files and subfiles in path
        private static void FullDirList(DirectoryInfo dir, string searchPattern, bool subdirs)
        {
            // list the files
            try
            {
                foreach (FileInfo f in dir.GetFiles(searchPattern))
                {
                    files.Add(new ImageStruct(f));
                }
            }
            catch
            {
                return;  // We alredy got an error trying to access dir so dont try to access it again
            }

            // process each directory
            // If I have been able to see the files in the directory I should also be able 
            // to look at its directories so I dont think I should place this in a try catch block
            if (subdirs)
            {
                foreach (DirectoryInfo d in dir.GetDirectories())
                {
                    FullDirList(d, searchPattern, true);
                }
            }
        }


        private sealed class Options : CommandLineOptionsBase
        {
            [Option("d", "directory", HelpText = "Directory where to load images.", Required = true)]
            public string Directory { get; set; }

            [Option("r", "recursive", HelpText = "Search subdirectories for images.")]
            public bool Recursive { get; set; }

            [Option("l", "large", HelpText = "Use 256 bit Hash.")]
            public bool LargeHash { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                var help = new HelpText();
                help.AddOptions(this);
                return help;
            }
        }
    }
}
