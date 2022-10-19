using System.Data;

namespace BlazorApp1.Helpers
{
    
    public class FilesAndDirectories 
    {
        private readonly string _projectName;
        private string _filePath;

        public FilesAndDirectories(SharedDataService dataService)
        {
            _projectName = dataService.ProjectPath.FullName;
        }

        public String CreateImagePath(string CollectionId, string  FileName)
        {
            // Get the extension
            var extension = Path.GetExtension(FileName);

            // Validate the extension based on your business needs

            // Generate a new file to avoid dublicates = (FileName withoutExtension - GUId.extension)
            var newFileName = $"{Guid.NewGuid().ToString()}{extension}";

            if (Directory.Exists(_projectName + "/" + CollectionId + "/images/"))
            {
                _filePath = _projectName + "/" + CollectionId + "/images/" + newFileName;

            }
            else
            {
                Directory.CreateDirectory(_projectName + "/" + CollectionId + "/images/");
                _filePath = _projectName + "/" + CollectionId + "/images/" + newFileName;

            }
            return _filePath;

        }

        public async Task SaveImageLocaly(string FilePath, Stream Stream)
        {
           
                FileStream fs = File.Create(FilePath);
                await Stream.CopyToAsync(fs);

                Stream.Close();
                fs.Close();
           

        }
        public async Task SaveImageLocaly(string FilePath, byte[] bytes)
        {
            await File.WriteAllBytesAsync(FilePath, bytes);
        }
    }
}
