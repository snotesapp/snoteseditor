using BlazorApp1.Data;
using BlazorApp1.Helpers;
using BlazorApp1.Services;
using Microsoft.JSInterop;
using System.IO.Compression;
using System.Text.Json;
using ReactiveUI;
using Microsoft.AspNetCore.Components.Forms;



namespace BlazorApp1.ViewModels
{
    public class ProjectViewModel : ReactiveObject
    {
        private readonly ProjectService Project_service;
        private readonly SharedDataService SharedDataService_service;

        private readonly IJSRuntime jSRuntime_JS;
       
        public ProjectViewModel(SharedDataService sharedDataService, ProjectService projectService, IJSRuntime jsRuntime )
        {
          
            this.SharedDataService_service = sharedDataService;
            this.Project_service = projectService;
            this.jSRuntime_JS = jsRuntime;
            
        }

        private bool _drawer = true;
        public bool Drawer
        {
            get { return _drawer; }
            set { this.RaiseAndSetIfChanged(ref _drawer, value); }
        }

        private bool _mini = true;
        public bool Mini
        {
            get { return _mini; }
            set { this.RaiseAndSetIfChanged(ref _mini, value); }
        }

        public bool ConfirmLeave { get; set; } = false;

        public async Task<Project> GetProject()
        {
            return await Project_service.GetProject();

        }

        public async Task<Project> GetFullProject()
        {
            return await Project_service.GetFullProject();
        }


        public async Task UpdateProject()
        {

            await Project_service.UpdateProject(SharedDataService_service.MainProject);

        }

        public async Task InsertProject(Project project)
        {

            await Project_service.InsertProject(project);
            NotifyStateChanged();

        }

        public async Task RemoveSqliteCacheAsync()
        {
            try
            {
                await Project_service.RemoveSqliteCacheAsync();
            }
            catch (Exception)
            {

                Console.WriteLine("Can't remove sqlite cache");
            }
            
        }

        public async Task SqliteEnsureDeletedAsync()
        {
            try
            {
                await Project_service.SqliteEnsureDeletedAsync();
            }
            catch (Exception)
            {

                Console.WriteLine("Can't delete sqlite db");
            }
            
        }
        public async Task SqliteEnsureCreatedAsync()
        {
            try
            {
                await Project_service.SqliteEnsureCreatedAsync();
            }
            catch (Exception)
            {

                Console.WriteLine("Can't create sqlite db");
            }
            
        }

        public async Task<byte[]> GetSqliteCacheValueAsync()
        {
            return await Project_service.GetSqliteCacheValueAsync();

        }

        public async Task StoreSqliteCacheValueAsync(byte[] sqliteBytes)
        {
             await Project_service.StoreSqliteCacheValueAsync(sqliteBytes);
            

        }
        #region Open Project

        public async Task OpenProject(IBrowserFile browserFile)
        {
            await using FileStream fileContent = new(AppDomain.CurrentDomain.BaseDirectory + "Project.zip", FileMode.Create);
            await browserFile.OpenReadStream(browserFile.Size).CopyToAsync(fileContent);

            try
            {
                Loader = true;
                ExtractsNotesFile(fileContent);

                //  ZipFile.ExtractToDirectory(AppDomain.CurrentDomain.BaseDirectory + "Project.zip", AppDomain.CurrentDomain.BaseDirectory + "project");
            }
            catch
            {
                Console.WriteLine("Error");
            }

            await BuildProject();

            Loader = false;
            if (SharedDataService_service.MainProject.Packets.Count > 0)
            {
                SharedDataService_service.SwitchMenus("cards");
            }else{
                SharedDataService_service.SwitchMenus("collection");
            }
           
            
        }

        public async Task OpenProject()
        {
            await Task.Run(async () => {
                byte[] snotesfileArray = await jSRuntime_JS.InvokeAsync<byte[]>("interop.OpenSnotesFile");

                using (var memoryStream = new MemoryStream(snotesfileArray))
                {

                    ExtractsNotesFile(memoryStream);
                }
            

            }).ContinueWith(async t =>{
                   await BuildProject();
                    Loader = false;
            if (SharedDataService_service.MainProject.Packets.Count > 0)
            {
                SharedDataService_service.SwitchMenus("cards");
            }else{
                SharedDataService_service.SwitchMenus("collection");
            }

           

            });
           
            
            //await BuildProject();

            
           
        }

        public async Task BuildProject()
        {

            if (SharedDataService_service.ProjectPath is null || SharedDataService_service.ProjectPath.Exists == false)
            {
                SharedDataService_service.ProjectPath = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "project/collections");
            }



            var jsonProjectFile = SharedDataService_service.ProjectPath.Parent.FullName + "/jsonFile.json";

            if (File.Exists(jsonProjectFile))
            {
                await RemoveSqliteCacheAsync();
                await SqliteEnsureDeletedAsync();
                await SqliteEnsureCreatedAsync();
                using (FileStream openStreamPrj = File.OpenRead(jsonProjectFile))
                {
                    Project? newProject =
                      await JsonSerializer.DeserializeAsync<Project>(openStreamPrj);
                    SharedDataService_service.MainProject = newProject;
                }


                await InsertProject(SharedDataService_service.MainProject);

                var jsonNoteCardsFile = SharedDataService_service.ProjectPath.Parent.FullName + "/notecards.json";

                using (FileStream openStreamNC = File.OpenRead(jsonNoteCardsFile))
                {
                    List<NotePacket>? noteCardsList =
                      await JsonSerializer.DeserializeAsync<List<NotePacket>>(openStreamNC);
                    await SharedDataService_service.NewRangNoteCards(noteCardsList);
                }

                SharedDataService_service.MainProject = await GetFullProject();

                File.Delete(jsonProjectFile);
                File.Delete(jsonNoteCardsFile);
                File.Delete(SharedDataService_service.ProjectPath.Parent.FullName + "/snotesonline.db");

            }
            else
            {
                await RemoveSqliteCacheAsync();

                byte[] sqlitebytes = await File.ReadAllBytesAsync(SharedDataService_service.ProjectPath.Parent.FullName + "/snotesonline.sqlite3");
                var sourse = SharedDataService_service.ProjectPath.Parent.FullName + "/snotesonline.sqlite3";
                var dest = AppDomain.CurrentDomain.BaseDirectory + "snotesonline.sqlite3";
                File.Copy(sourse, dest, true);

                await StoreSqliteCacheValueAsync(sqlitebytes);



                SharedDataService_service.MainProject = await GetProject();
            }

           

        }
        public void ExtractsNotesFile(Stream snFileStream)
        {
            var extractPath = "/project";

            using (ZipArchive archive = new ZipArchive(snFileStream, ZipArchiveMode.Read))
                {
                    // Ensure the extraction directory exists
                    Directory.CreateDirectory(extractPath);
                    Directory.SetCurrentDirectory(extractPath);

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {

                        // Create directory for the entry if it doesn't exist
                        string entryDirectory = Path.GetDirectoryName(entry.FullName);
                        // create directory
                        if (!string.IsNullOrEmpty(entryDirectory) && !Directory.Exists(entryDirectory))
                        {
                            Directory.CreateDirectory(entryDirectory);
                        }

                        // Extract only if it's not a directory
                        if (!string.IsNullOrEmpty(entry.Name))
                        {
                            // Full path for the entry
                            string entryFilePath = Path.Combine(entryDirectory, entry.Name);

                            // Open a FileStream for the entry
                            using (FileStream fileStream = File.Create(entryFilePath))
                            {
                                // Copy the contents of the entry to the FileStream
                                using (Stream entryStream = entry.Open())
                                {
                                    entryStream.CopyTo(fileStream);
                                }
                            }


                        }

                    }
                }



            Directory.SetCurrentDirectory("/");
        }

        #endregion

        #region Download/Save Project


        public byte[] fileArray;

        private async Task<string> CreatePkgFile()
        {
            
            string packageFilePath = AppDomain.CurrentDomain.BaseDirectory + FilePaths.PackageDirectory + "/" + FilePaths.PackageFilePath;

            if (File.Exists(packageFilePath))
            {
                File.Delete(packageFilePath);
            }


            // Create the package directory
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + FilePaths.PackageDirectory);

            return packageFilePath;

        }

        private async Task<byte[]> BuildProjectPackage()
        {
            
          
            string packageFilePath = await CreatePkgFile();

            var sqliteFilePath = SharedDataService_service.ProjectPath.Parent.FullName + "/" + FilePaths.SQlliteFile;
            byte[] sqliteFile = await GetSqliteCacheValueAsync();
            File.WriteAllBytes(sqliteFilePath, sqliteFile);

            
            // Create the meta object and serialize it to a byte array
            var metaObject = new MetaObject
            {
                Version = "1.0"
            };
            await SerializeMetaObject(metaObject);

            string projectRootPath = SharedDataService_service.ProjectPath.Parent.FullName;
            // Create the package file
            System.IO.Compression.ZipFile.CreateFromDirectory(projectRootPath, packageFilePath);

            // Read the package file into a byte array
            using (var packageFile = new FileStream(packageFilePath, FileMode.Open))
            {
                fileArray = new byte[packageFile.Length];
                await packageFile.ReadAsync(fileArray, 0, (int)packageFile.Length);
            }


            return fileArray;
        }

        public async Task SaveProject()
        {
           
         

           ConfirmLeave = false;
           Loader = true;
           fileArray = await BuildProjectPackage();
           Loader = false;

           // await SetProgres(50);
           try
           {

               await jSRuntime_JS.InvokeVoidAsync("interop.getNewFileHandle", SharedDataService_service.MainProject.Name);

               await jSRuntime_JS.InvokeVoidAsync("interop.blazorSaveFile", "application/zip", fileArray);

               Loader = false;

           }
           catch (JSException ex)
           {
               if (ex.Message.Contains("User activation is required to request permissions."))
               {

                   // Handle the specific error
                   Console.WriteLine("Error: User activation is required to request permissions.");

                   Loader = false;


               }
               else
               {
                   // Handle other JSExceptions
                   Console.WriteLine("JSException: " + ex.Message);
               }
           }

          

        }

        public async Task DownloadProjectFile()
        {
            try
            {
                ConfirmLeave = false;
                Loader = true;

                byte[] ProjectArray = await BuildProjectPackage();


                var fileName = $"{SharedDataService_service.MainProject.Name}.snotes";

                await jSRuntime_JS.InvokeVoidAsync("interop.blazorDownloadFile", fileName, "application/zip", ProjectArray);

                // Delete the package file
                var packageFilePath = AppDomain.CurrentDomain.BaseDirectory + FilePaths.PackageDirectory + "/" + FilePaths.PackageFilePath;


                if (Path.HasExtension(packageFilePath))
                {
                    File.Delete(packageFilePath);
                }

                Loader = false;

            }
            catch (Exception)
            {

                Loader = false;
            }
           

   
           
        }

        private async Task SerializeMetaObject(MetaObject metaObject)
        {


            string fileName = SharedDataService_service.ProjectPath.Parent.FullName + "/" + FilePaths.MetaFilePath;
            
            await using (FileStream createStream = File.Create(fileName))
            {
                await JsonSerializer.SerializeAsync(createStream, metaObject);
            }
            
           
           
        }

       

        #endregion



       

        #region JavaScript
    
        private bool _loader = false;
        public bool Loader
        {
            get { return _loader; }
            set { _loader = value; NotifyStateChanged(); }
        }


        #endregion



        public event Action OnChange;
        public void NotifyStateChanged() => OnChange?.Invoke();

    }
}
