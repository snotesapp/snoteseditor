using BlazorApp1.Data;
using BlazorApp1.Helpers;
using BlazorApp1.Services;
using BlazorBootstrap;
using DynamicData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.JSInterop;
using System;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using ReactiveUI;
using BlazorComponent;
using SqliteWasmHelper;

namespace BlazorApp1.ViewModels
{
    public class ProjectViewModel : ReactiveObject
    {
        private readonly ProjectService Project_service;
        private readonly SharedDataService SharedDataService_service;
        private readonly IJSRuntime jSRuntime_JS;
       


        public ProjectViewModel(SharedDataService sharedDataService, ProjectService projectService, IJSRuntime jsRuntime)
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
            await Project_service.RemoveSqliteCacheAsync();
        }

        public async Task SqliteEnsureDeletedAsync()
        {
            await Project_service.SqliteEnsureDeletedAsync();
        }
        public async Task SqliteEnsureCreatedAsync()
        {
            await Project_service.SqliteEnsureCreatedAsync();
        }

        public async Task<byte[]> GetSqliteCacheValueAsync()
        {
            return await Project_service.GetSqliteCacheValueAsync();

        }

        public async Task StoreSqliteCacheValueAsync(byte[] sqliteBytes)
        {
             await Project_service.StoreSqliteCacheValueAsync(sqliteBytes);
            

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

            SharedDataService_service.newProjectDialog = false;

        }


        #region Download Project

        
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
            ZipFile.CreateFromDirectory(projectRootPath, packageFilePath);

            // Read the package file into a byte array
            using (var packageFile = new FileStream(packageFilePath, FileMode.Open))
            {
                fileArray = new byte[packageFile.Length];
                await packageFile.ReadAsync(fileArray, 0, (int)packageFile.Length);
            }


            return fileArray;
        }

        public async Task OpenProject()
        {

            byte[] snotesfileArray = await jSRuntime_JS.InvokeAsync<byte[]>("interop.OpenSnotesFile");
            using (var memoryStream = new MemoryStream(snotesfileArray))
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

            await BuildProject();

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
