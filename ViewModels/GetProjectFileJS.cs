using BlazorApp1.Helpers;
using Microsoft.JSInterop;
using System.Data;
using System.IO.Compression;

namespace BlazorApp1.ViewModels
{
    public class GetProjectFileJS
    {

        private readonly SharedDataService _dataSvs;
        public GetProjectFileJS(SharedDataService dataSvs)
        {
            _dataSvs = dataSvs;
        }


        [JSInvokable]
        public async Task UpdateFileArray(string base64)
        {
           
                _dataSvs.CurrentStep = "Progress";

                var fileArray = Convert.FromBase64String(base64);

                using (var memoryStream = new MemoryStream(fileArray))
                using (var zipArchive = new ZipArchive(memoryStream))
                {
                    var extractPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "project");
                    zipArchive.ExtractToDirectory(extractPath);


                }


                await _dataSvs.BuildProject();
                Console.WriteLine("File Extracted");
            }
    
        

       
        // other methods...
    }
}
