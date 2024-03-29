﻿namespace Phishy.Utils
{
    internal class FileUtils
    {
        public static bool FileExistsInCurrentDirectory(string fileName)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string filePath = Path.Combine(currentDirectory, fileName);

            return File.Exists(filePath);
        }

        public static void SaveFileInCurrentDirectory(string fileName, string yaml)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string filePath = Path.Combine(currentDirectory, fileName);

            File.WriteAllText(filePath, yaml);
        }

        public static string ReadFileFromCurrentDirectory(string fileName)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string filePath = Path.Combine(currentDirectory, fileName);

            return File.ReadAllText(filePath);
        }
    }
}
