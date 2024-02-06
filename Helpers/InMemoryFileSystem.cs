namespace BlazorApp1.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;

    public class InMemoryFileSystem
    {
        private Dictionary<string, string> files; // Simulate files with content
        private HashSet<string> directories;      // Simulate directories

        public InMemoryFileSystem()
        {
            files = new Dictionary<string, string>();
            directories = new HashSet<string>();
        }

        // Create a new file with its parent directories
        public void CreateFile(string path, string content)
        {
            string directory = Path.GetDirectoryName(path);
            CreateDirectory(directory); // Ensure parent directories exist
            files[path] = content;
        }

        // Create a new directory with its parent directories
        public void CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid directory path.");
            }

            if (!directories.Contains(path))
            {
                directories.Add(path);
            }
        }


        // Check if a file exists
        public bool FileExists(string path)
        {
            return files.ContainsKey(path);
        }

        // Check if a directory exists
        public bool DirectoryExists(string path)
        {
            return directories.Contains(path);
        }

        // Read the content of a file
        public string ReadFile(string path)
        {
            if (FileExists(path))
            {
                return files[path];
            }
            else
            {
                throw new FileNotFoundException($"File {path} not found.");
            }
        }

        // Delete a file
        public void DeleteFile(string path)
        {
            if (FileExists(path))
            {
                files.Remove(path);
            }
        }

        // Delete a directory
        public void DeleteDirectory(string path)
        {
            if (DirectoryExists(path))
            {
                directories.Remove(path);
            }
        }

        // Zip a folder
        // Zip a folder
        public byte[] ZipFolder(string folderPath)
        {
            if (!DirectoryExists(folderPath))
            {
                throw new DirectoryNotFoundException($"Directory {folderPath} not found.");
            }

            using (MemoryStream zipStream = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
                    {
                        string entryName = file.Substring(folderPath.Length + 1).Replace("\\", "/");
                        archive.CreateEntryFromFile(file, entryName);
                    }
                }
                return zipStream.ToArray();
            }
        }


        // Extract zip content to the given directory
        public void ExtractZipContent(byte[] zipContent, string destinationDirectory)
        {
            using (MemoryStream zipStream = new MemoryStream(zipContent))
            {
                using (ZipArchive archive = new ZipArchive(zipStream))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string entryPath = Path.Combine(destinationDirectory, entry.FullName);
                        string entryDirectory = Path.GetDirectoryName(entryPath);
                        if (!Directory.Exists(entryDirectory))
                        {
                            Directory.CreateDirectory(entryDirectory);
                        }
                        entry.ExtractToFile(entryPath, true);
                    }
                }
            }
        }

    }

}
