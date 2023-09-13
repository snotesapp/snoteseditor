using BlazorApp1.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.IO.Compression;

namespace BlazorApp1.ViewModels
{
    public class GetProjectFileJS
    {

        private readonly SharedDataService _dataSvs;
        private readonly ProjectViewModel ProjectVM;

        public GetProjectFileJS(SharedDataService dataSvs, ProjectViewModel projectVM)
        {
            _dataSvs = dataSvs;
            ProjectVM = projectVM;
        }


        [JSInvokable]
        public async Task UpdateFileArray(byte[] base64)
        {

            // _dataSvs.CurrentStep = "Progress";

           

            byte[] fileArray = base64;
            
            using (var memoryStream = new MemoryStream(fileArray))
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    if (archive.Entries.Count == 0)
                    {
                        Console.WriteLine("The contents of the base64 string is not a valid zip file. ");
                        return;
                    }

                    var extractPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "project");
                    archive.ExtractToDirectory(extractPath);
                }
            }

         

            await ProjectVM.BuildProject();
            

        }


        [JSInvokable]
        public bool SetLoaderValue(bool value)
        {
            ProjectVM.Loader = value;
            return value;
        }



    }
}
