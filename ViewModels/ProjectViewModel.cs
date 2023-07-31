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



namespace BlazorApp1.ViewModels
{
    public class ProjectViewModel : ReactiveObject
    {
        private readonly ProjectService Project_service;
        private readonly SharedDataService SharedDataService_service;
        private readonly IJSRuntime jSRuntime_JS;

        public Modal? newProjectModal;
        public ProjectViewModel(SharedDataService sharedDataService,ProjectService projectService,IJSRuntime jsRuntime)
        {
            this.SharedDataService_service = sharedDataService;
            this.Project_service = projectService;
            this.jSRuntime_JS = jsRuntime;
        }

        private bool _drawer =true;
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



        public async Task BuildProject()
        {
           
            if (SharedDataService_service.ProjectPath is null || SharedDataService_service.ProjectPath.Exists == false)
            {
                SharedDataService_service.ProjectPath = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "project/collections");
            }


            var jsonProjectFile = SharedDataService_service.ProjectPath.Parent.FullName + "/jsonFile.json";

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

           

            SharedDataService_service.newProjectDialog = false;

        }


        #region Download Project

        private static readonly JsonSerializerOptions ProjectOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            IgnoreReadOnlyProperties = true,
        };

        private static readonly JsonSerializerOptions NoteCardsOptions = new()
        {
            WriteIndented = true,
        };
        
        public byte[] fileArray;
        private async Task<byte[]> BuildProjectPackage()
        {
            var packageFilePath = AppDomain.CurrentDomain.BaseDirectory + FilePaths.PackageDirectory + "/" + FilePaths.PackageFilePath;

            if (File.Exists(packageFilePath))
            {
                File.Delete(packageFilePath);
            }


            // Create the package directory
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + FilePaths.PackageDirectory);
           

            // Create the meta object and serialize it to a byte array
            var metaObject = new MetaObject
            {
                Version = "1.0"
            };
            byte[] metaJsonBytes = SerializeMetaObject(metaObject);
          

            // Write the serialized meta object to a file
            File.WriteAllBytes(SharedDataService_service.ProjectPath.Parent.FullName + "/" + FilePaths.MetaFilePath, metaJsonBytes);


            // Serialize the full project object to a file
            Project fullPrjct = await GetFullProject();
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

            // Create the package file
           

            ZipFile.CreateFromDirectory(SharedDataService_service.ProjectPath.Parent.FullName, packageFilePath);

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

        private byte[] SerializeMetaObject(MetaObject metaObject)
        {
            return JsonSerializer.SerializeToUtf8Bytes(metaObject);
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
