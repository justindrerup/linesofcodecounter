using NLog;
using System;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Text.RegularExpressions;

namespace LinesOfCodeCounter
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static long LinesOfCode { get; set; }
        private static string[] AllowedFileExtensions { get; set; }
        private static string[] ExcludedFolders { get; set; }

        static void Main(string[] args)
        {
            var options = new Options();

            var isValid = CommandLine.Parser.Default.ParseArgumentsStrict(args, options);

            if (isValid)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(options.Path);

                Setup();

                ProcessDirectoryTree(directoryInfo);

                logger.Info($"Total Lines of code: {string.Format("{0:n0}", LinesOfCode)}");
            }
            else
            {
                logger.Info("Invalid CommandLine arguments");
            }
        }

        static void Setup()
        {
            var extensions = ConfigurationManager.AppSettings["AllowedFileExtensions"];
            var folders = ConfigurationManager.AppSettings["ExcludedFolders"];

            AllowedFileExtensions = Regex.Split(extensions, ",");
            ExcludedFolders = Regex.Split(folders, ",");

            logger.Info("File extensions included in line count");

            foreach (var item in AllowedFileExtensions)
            {
                logger.Trace(item);
            }

            logger.Info("Folders excluded in line count");

            foreach (var item in ExcludedFolders)
            {
                logger.Trace(item);
            }
        }

        static void ProcessDirectoryTree(DirectoryInfo root)
        {
            int fileCount = 0;

            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles("*.*");
            }

            // This is thrown if even one of the files requires permissions greater than the application provides.
            catch (UnauthorizedAccessException e)
            {
                logger.Trace(e.Message);
            }

            catch (DirectoryNotFoundException e)
            {
                logger.Trace(e.Message);
            }

            if (files != null)
            {
                foreach (FileInfo fileInfo in files)
                {
                    if (AllowedFileExtensions.Contains(fileInfo.Extension))
                    {
                        fileCount++;

                        logger.Trace(fileInfo.FullName);

                        long lineCount = CountLinesInFile(fileInfo.FullName);
                        LinesOfCode = LinesOfCode + lineCount;

                        logger.Info($"Lines of code for {fileInfo.Name}: {lineCount}");
                    }
                }

                // Reset the file count
                fileCount = 0;

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (DirectoryInfo dirInfo in subDirs)
                {
                    // Recursive call for each subdirectory.
                    if (!ExcludedFolders.Contains(dirInfo.Name))
                    {
                        logger.Info($"Processing folder '{dirInfo.Name}'");

                        ProcessDirectoryTree(dirInfo);
                    }
                }
            }
        }

        private static long CountLinesInFile(string file)
        {
            long count = 0;
            using (StreamReader r = new StreamReader(file))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    // TODO: Add check to see if the line being counted is a line of code or a comment
                    count++;
                }
            }
            return count;
        }
    }
}
