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
        private readonly IBrowserCache _browserCache;


        public ProjectViewModel(SharedDataService sharedDataService, ProjectService projectService, IJSRuntime jsRuntime, IBrowserCache browserCache)
        {
            this.SharedDataService_service = sharedDataService;
            this.Project_service = projectService;
            this.jSRuntime_JS = jsRuntime;
            this._browserCache = browserCache;
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

        public bool NewProject { get; set; } = false;

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

            /*
            var files = Directory.GetFiles(SharedDataService_service.ProjectPath.Parent.FullName);
            var files2 = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);
            var sourse = SharedDataService_service.ProjectPath.Parent.FullName + "/snotesonline.db";
            var dest = AppDomain.CurrentDomain.BaseDirectory + "snotesonline2.db";
            //File.Copy(sourse, dest, true);
            Project_service.CopyDB(sourse, dest);
            SharedDataService_service.MainProject = await GetProject();
            */


            var jsonProjectFile = SharedDataService_service.ProjectPath.Parent.FullName + "/jsonFile.json";

            using (FileStream openStreamPrj = File.OpenRead(jsonProjectFile))
            {
                Project? newProject =
                  await JsonSerializer.DeserializeAsync<Project>(openStreamPrj);
               // SharedDataService_service.MainProject = newProject;
            }


            //await InsertProject(SharedDataService_service.MainProject);

            var jsonNoteCardsFile = SharedDataService_service.ProjectPath.Parent.FullName + "/notecards.json";

            using (FileStream openStreamNC = File.OpenRead(jsonNoteCardsFile))
            {
                List<NotePacket>? noteCardsList =
                  await JsonSerializer.DeserializeAsync<List<NotePacket>>(openStreamNC);
               // await SharedDataService_service.NewRangNoteCards(noteCardsList);
            }


            await Task.Delay(500);
            await RemoveSqliteCacheAsync();
            await Task.Delay(500);
            byte[] sqlitebytes= await File.ReadAllBytesAsync(SharedDataService_service.ProjectPath.Parent.FullName + "/snotesonline.sqlite3");
            await Task.Delay(1000);

            //var resp = await _browserCache.SyncDbWithCacheAsync("snotesonline.sqlite3"); 
            await Project_service.SqliteEnsureCreatedAsync();
            await StoreSqliteCacheValueAsync(sqlitebytes);
            await Task.Delay(500);
            SharedDataService_service.MainProject = await GetFullProject();
            SharedDataService_service.newProjectDialog = false;

        }


        #region Download Project

        private static readonly JsonSerializerOptions ProjectOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false,
            IgnoreReadOnlyProperties = true,
        };

        private static readonly JsonSerializerOptions NoteCardsOptions = new()
        {
            WriteIndented = false,
        };
        
        public byte[] fileArray;

        private async Task<string> CreatePkgFile()
        {
            await Task.Delay(500);
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
          

            // Write the serialized meta object to a file
            //File.WriteAllBytes(SharedDataService_service.ProjectPath.Parent.FullName + "/" + FilePaths.MetaFilePath, metaJsonBytes);


            // Serialize the full project object to a file
            
            Project fullPrjct = await GetFullProject();
            //Project fullPrjct = SharedDataService_service.MainProject;
            if (fullPrjct != null)
            {
                await SerializeProjectAsync(fullPrjct);
            }

            // Serialize the list of note cards to a file
            List<NotePacket> allNoteCards = await SharedDataService_service.GetAllNoteCards();
            if (allNoteCards != null)
            {
                await SerializeNoteCardsAsync(allNoteCards);
            }


            string projectRootPath = SharedDataService_service.ProjectPath.Parent.FullName;

            //string mydbfile = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory).
            //    Where(file => Path.GetFileName(file).Equals("snotesonline.db", StringComparison.OrdinalIgnoreCase)).
            //    First();
           
            //File.Copy(mydbfile, projectRootPath + $"{mydbfile}", true);

          

            /*
                        List<string> dbfiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory).Where( ob => ob.Contains("snotesonline")).ToList();

                       foreach (string dbfile in dbfiles)
                        {
                            File.Copy(dbfile, projectRootPath + $"{dbfile}", true);
                        }

                        var folderfils = Directory.GetFiles(projectRootPath);
              */


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

        private async Task SerializeProjectAsync(Project project)
        {
            var jsonProjectFilePath = SharedDataService_service.ProjectPath.Parent.FullName + "/" + FilePaths.ProjectFilePath;
            await using (FileStream createProjectStream = File.Create(jsonProjectFilePath))
            {
                await JsonSerializer.SerializeAsync<Project>(createProjectStream, project, ProjectOptions);
            }
        }
        private async Task SerializeNoteCardsAsync(List<NotePacket> noteCards)
        {
            var jsonNoteCardsFilePath = SharedDataService_service.ProjectPath.Parent.FullName + "/" + FilePaths.NoteCardsFilePath;
            await using (FileStream createNotecardsStream = File.Create(jsonNoteCardsFilePath))
            {
                await JsonSerializer.SerializeAsync<List<NotePacket>>(createNotecardsStream, noteCards, NoteCardsOptions);
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
