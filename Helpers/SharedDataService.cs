using BlazorApp1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using ReactiveUI;
using SkiaSharp;
using SkiaSharp.Views.Blazor;
using System.Data;
using System.IO.Compression;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Util.Reflection.Expressions;

namespace BlazorApp1.Helpers
{

    public class SharedDataService : ReactiveObject
    {
        private string _searchTerm;
        public string SearchTerm
        {
            get => _searchTerm;
            set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
        }


    

        #region global parameters


        public event Action OnChange;

        public Microsoft.AspNetCore.Components.Forms.IBrowserFile ProjectUploadedfile;

        public string menustatus { get; private set; } = "mainmenu";
        public bool ncDragingMode = false;

        public int menuCols = 2;
        public int bodyCols = 10;

        public bool newProjectDialog;

        public bool noteEdited;


        private bool _showFilterNotes = false;
        public bool showFilterNotes
        {
            get => _showFilterNotes;
            set
            {
                this.RaiseAndSetIfChanged(ref _showFilterNotes, value);
                NotifyStateChanged();
            }
        }

        // public bool savenotedialog = false;

        private bool _savenotedialog;
        public bool savenotedialog
        {
            get { return _savenotedialog; }
            set { this.RaiseAndSetIfChanged(ref _savenotedialog, value); NotifyStateChanged(); }

        }

        private bool _download_dialog;
        public bool download_dialog
        {
            get { return _download_dialog; }
            set { this.RaiseAndSetIfChanged(ref _download_dialog, value); NotifyStateChanged(); }

        }
        private bool _savingProject;
        public bool SavingProject
        {
            get { return _savingProject; }
            set { this.RaiseAndSetIfChanged(ref _savingProject, value); NotifyStateChanged(); }

        }
        public string CurrentStep { get; set; } = "Project";


        private string _userAgent ;
        public string? UserAgent

        {
            get { return _userAgent; }
            set { this.RaiseAndSetIfChanged(ref _userAgent, value); NotifyStateChanged(); }

        }

        #endregion




        public PaintMode paintMode = PaintMode.Drag;

        public string noteBackgroundColor = "#FFFFFF";

        //Note Props
        public Note editNote = new Note()
        {
            Text = "",
            Images = new List<NoteImage>(),
            NotePaths = new List<NotePath>(),
            

        };

       
        public List<NoteImage> imagelist = new List<NoteImage>();

        
        private List<Note> _selectedNCNotes= new List<Note>();
        public List<Note> selectedNCNotes 
        {
            get => _selectedNCNotes;
            set => this.RaiseAndSetIfChanged(ref _selectedNCNotes, value);
        }



        public Project MainProject = new Project()
        {
            
            Packets = new List<Packet>(),
            NotesCollection = new List<NotesCollection>() ,
        };

        public DirectoryInfo ProjectPath { get; set; }


        public NotesCollection AddNotesSelectedNC;

        //public Packet? SelectedCard;
        private Packet _selectedCard;
        public Packet? SelectedCard
        {
            get { return _selectedCard; }
            set { this.RaiseAndSetIfChanged(ref _selectedCard, value); NotifyStateChanged(); }
        }


        public List<Packet> AllCards;
        public List<Packet> SelectionCards = new();
        public List<Packet> ChildCards = new();

        public WindowDimension Wdimension = new();

       // public NotePacket? SelectedNoteCard;
        private NotePacket _selectedNoteCard;
        public NotePacket? SelectedNoteCard
        {
            get { return _selectedNoteCard; }
            set { this.RaiseAndSetIfChanged(ref _selectedNoteCard, value); NotifyStateChanged(); }
        }


        private IDbContextFactory<SNotesDBContext> _dbContextFactory { get; set; }
        
        public SharedDataService(IDbContextFactory<SNotesDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
               using var db =  _dbContextFactory.CreateDbContext();
               db.Database.EnsureCreatedAsync();

        }

        public void SwitchMenus(string selectedMenu)
        {
           
                menustatus = selectedMenu;
                NotifyStateChanged();
           
            
           
        }

        #region Project Crud
        public async Task<bool> InsertProject(Project project)
        {
            using (var projectsContext = _dbContextFactory.CreateDbContext())
            {
              // projectsContext.Add(project);
                await projectsContext.AddAsync(project);
                await projectsContext.SaveChangesAsync();
                NotifyStateChanged();
            }

            return true;
        }

        public async Task<Project> GetProject()
        {
            using (var projectsContext = _dbContextFactory.CreateDbContext())
            {
                Project  mProject = await projectsContext.Projects.Include(nc => nc.NotesCollection).Include(cr => cr.Packets.Where(s => s.Selected == true)).ThenInclude(pc => pc.Parent).FirstOrDefaultAsync();
                return mProject;


            }
            
        }


        public async Task<bool> UpdateProject()
        {
            
            using (var projectsContext = _dbContextFactory.CreateDbContext())
            {
                var tracking = projectsContext.Update(MainProject);
                await projectsContext.SaveChangesAsync();

                var isModified = tracking.State == EntityState.Modified;
                return isModified;
            }
        }

        public async Task<Project> GetFullProject()
        {
            using (var projectsContext = _dbContextFactory.CreateDbContext())
            {

                 return await projectsContext.Projects.Include(nc => nc.NotesCollection).ThenInclude(ncn => ncn.Note).ThenInclude(nci => nci.Images)
                    .Include(nc => nc.NotesCollection).ThenInclude(ncn => ncn.Note).ThenInclude(pth => pth.NotePaths)
                    .Include(cr => cr.Packets).FirstOrDefaultAsync();
               // return await projectsContext.Projects.Include(nc => nc.NotesCollection).Include(cr => cr.Cards).FirstOrDefaultAsync();

            }
        }

        #endregion

        #region Packet ViewModel
        private string _filterPacketsTxt;
        public string FilterPacketsTxt
        {
            get => _filterPacketsTxt;
            set => this.RaiseAndSetIfChanged(ref _filterPacketsTxt, value);
        }


        private bool _filterPackets;
        public bool filterPackets
        {
            get { return _filterPackets; }
            set { this.RaiseAndSetIfChanged(ref _filterPackets, value); NotifyStateChanged(); }

        }



        private bool _showDeletePacketConfirmation;
        public bool showDeletePacketConfirmation
        {
            get { return _showDeletePacketConfirmation; }
            set { this.RaiseAndSetIfChanged(ref _showDeletePacketConfirmation, value); NotifyStateChanged(); }

        }


        private bool _moveto_dialog;
        public bool moveto_dialog
        {
            get { return _moveto_dialog; }
            set { this.RaiseAndSetIfChanged(ref _moveto_dialog, value); NotifyStateChanged(); }

        }


        public Packet? ContextMenuCard = null;

        private List<Packet> _filtredPackets = new List<Packet>();
        public List<Packet> FiltredPackets
        {
            get => _filtredPackets;
            set => this.RaiseAndSetIfChanged(ref _filtredPackets, value);
        }

   
        #endregion



        #region NoteCards Crud

        public async Task<NotePacket> GetNoteCard(NotePacket noteCard)
        {
            using (var notecardsContext = _dbContextFactory.CreateDbContext())
            {
                NotifyStateChanged();
                return await notecardsContext.NotePackets.Where(ncd => ncd.NoteID == noteCard.NoteID && ncd.PacketID == noteCard.PacketID).Include(nt => nt.Note).ThenInclude(im => im.Images).FirstOrDefaultAsync();
            }
        }

        public async Task <List<NotePacket>> GetAllNoteCards()
        {
            using (var notecardsContext = _dbContextFactory.CreateDbContext())
            {
                return await notecardsContext.NotePackets.ToListAsync();

            }
        }

        public async Task<bool> NewNoteCard(Packet addCard, Note addNote)
        {
            using (var notecardsContext = _dbContextFactory.CreateDbContext())
            {
                //cardsContext.Note.Add(NewNote);
                //await cardsContext.SaveChangesAsync();
                // cardsContext.NoteCards.Add(new NoteCard { Card =NewCard , Note = NewNote, Order = NewCard.NoteCards.Count });
                NotePacket noteCard = new NotePacket() { PacketID = addCard.PacketID, NoteID = addNote.NoteID };

                try
                {
                    if (!notecardsContext.NotePackets.Contains(noteCard))
                    {
                        notecardsContext.NotePackets.Add(new NotePacket { PacketID = addCard.PacketID, NoteID = addNote.NoteID });
                        await notecardsContext.SaveChangesAsync();
                    }
                  
                }
                catch (DbUpdateException e)
                {
                    
                    Console.WriteLine(e.Message);
                }
             


            }
            return true;
        }
        public async Task<bool> NewNoteCard(int cardID, int noteID)
        {
            using (var notecardsContext = _dbContextFactory.CreateDbContext())
            {
                //cardsContext.Note.Add(NewNote);
                //await cardsContext.SaveChangesAsync();
                // cardsContext.NoteCards.Add(new NoteCard { Card =NewCard , Note = NewNote, Order = NewCard.NoteCards.Count });
                NotePacket noteCard = new NotePacket() { PacketID = cardID, NoteID = noteID };

                try
                {
                    if (!notecardsContext.NotePackets.Contains(noteCard))
                    {
                        notecardsContext.NotePackets.Add(new NotePacket() { PacketID = cardID, NoteID = noteID });
                        await notecardsContext.SaveChangesAsync();
                    }

                }
                catch (DbUpdateException e)
                {

                    Console.WriteLine(e.Message);
                }



            }
            return true;
        }


        public async Task<bool> AddRangNoteCard(Packet addCard, List<Note> addNotes)
        {
            using (var notecardsContext = _dbContextFactory.CreateDbContext())
            {
                foreach (Note addNote in addNotes)
                {
                    NotePacket noteCard = new NotePacket() { PacketID = addCard.PacketID, NoteID = addNote.NoteID };

                    try
                    {
                        if (!notecardsContext.NotePackets.Contains(noteCard))
                        {
                            notecardsContext.NotePackets.Add(new NotePacket { PacketID = addCard.PacketID, NoteID = addNote.NoteID });
                            await notecardsContext.SaveChangesAsync();
                        }

                    }
                    catch (DbUpdateException e)
                    {

                        Console.WriteLine(e.Message);
                    }

                }





            }
            return true;
        }

        public async Task<bool> NewRangNoteCards(List<NotePacket> noteCards)
        {
            using (var notecardsContext = _dbContextFactory.CreateDbContext())
            {

                await notecardsContext.AddRangeAsync(noteCards);
                await notecardsContext.SaveChangesAsync();


            }
            return true;
        }

        public async Task<bool> RemoveNoteCard(NotePacket noteCards)
        {
            using (var notecardsContext = _dbContextFactory.CreateDbContext())
            {

                notecardsContext.NotePackets.Remove(noteCards);
                await notecardsContext.SaveChangesAsync();
                NotifyStateChanged();

            }
            return true;
        }



        #endregion

        #region Notes Crud


        public async Task<List<Note>> GetNotes()
        {
            using (var notesContext = _dbContextFactory.CreateDbContext())
            {

                selectedNCNotes = await notesContext.Note.Where(nc => nc.NotesCollection.Selected == true).Take(20).ToListAsync();
                NotifyStateChanged();
                return selectedNCNotes;

            }
        }

        public async Task<bool> DeleteNotePaths(Note note)
        {
            using (var notesContext = _dbContextFactory.CreateDbContext())
            {
                notesContext.RemoveRange(note.NotePaths);
                await notesContext.SaveChangesAsync();

                NotifyStateChanged();

                return true;
            }
        }


        public async Task<bool> SaveNote()
        {
             noteBackgroundColor = editNote.BackgroundColor;
            using (var notesContext = _dbContextFactory.CreateDbContext())
            {
              
                editNote.NotesCollectionFK = AddNotesSelectedNC.NotesCollectionID;
                //   editNote.Images.AddRange(imagelist);
                if (!string.IsNullOrWhiteSpace(editNote.Text))
                {
                    notesContext.Add(editNote);
                    await notesContext.SaveChangesAsync();

                }

                //NotesCollection notesCollection = notesContext.NotesCollection.Where(p => p.ProjectFK == 1).Where(nc => nc.NotesCollectionID == 1).Include(n => n.Note).FirstOrDefault();

            }

            editNote = new Note()
            {
                NotesCollectionFK = AddNotesSelectedNC.NotesCollectionID,
                Images = new List<NoteImage>(),
                NotePaths = new List<NotePath>(),
                BackgroundColor = noteBackgroundColor,
                MainImgWidth = Wdimension.Width,
                MainImgHeight = Wdimension.Height
            };
            saveBitmap = new SKBitmap(Wdimension.Width, Wdimension.Height);
            // await GetNotes();
            NotifyStateChanged();

            return true;
        }

        public async Task<bool> UpdateNote()
        {

            using (var notesContext = _dbContextFactory.CreateDbContext())
            {

               
                notesContext.Update(editNote);
                await notesContext.SaveChangesAsync();
              


                //NotesCollection notesCollection = notesContext.NotesCollection.Where(p => p.ProjectFK == 1).Where(nc => nc.NotesCollectionID == 1).Include(n => n.Note).FirstOrDefault();

            }



            // await GetNotes();
            NotifyStateChanged();

            return true;
        }


        public async Task<List<Note>> GetNotes(int pageIndex = 0, int pageSize = 20)
        {
            if (pageIndex < 0)
            {
                throw new ArgumentOutOfRangeException("pageIndex must be greater than or equal to 0.");
            }
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException("pageSize must be greater than 0.");
            }

            try
            {
                using (var notesContext = _dbContextFactory.CreateDbContext())
                {
                    var selectedNotes = await notesContext.Note
                        .Where(nc => nc.NotesCollection.Selected == true)
                        .Skip(pageIndex )
                        .Take(pageSize)
                        .ToListAsync();

                    NotifyStateChanged();
                    return selectedNotes;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while fetching notes.", ex);
            }
        }


        public async Task<List<Note>> GetNotes(string NotesTextFilter)
        {
            using (var notesContext = _dbContextFactory.CreateDbContext())
            {
                selectedNCNotes = await notesContext.Note.Where(nt => nt.NotesCollection.Selected == true && nt.Text.ToLower().Contains(NotesTextFilter.Trim().ToLower())).Take(20).ToListAsync();
                NotifyStateChanged();
                return selectedNCNotes;

            }
        }

      
        public async Task<bool> DeleteNote(Note note)
        {
            using (var notesContext = _dbContextFactory.CreateDbContext())
            {
                var tracking = notesContext.Remove(note);
                await notesContext.SaveChangesAsync();
                var isDeleted = tracking.State == EntityState.Deleted;
                NotifyStateChanged();

                return isDeleted;
            }
        }


     
        public async Task<bool> UpdateNotePaths(Note note)
        {
            using (var notesContext = _dbContextFactory.CreateDbContext())
            {
                notesContext.UpdateRange(note.NotePaths);
                await notesContext.SaveChangesAsync();

                NotifyStateChanged();

                return true;
            }
        }

        public async Task<bool> DeleteNoteImg(NoteImage noteImage)
        {
            using (var notesContext = _dbContextFactory.CreateDbContext())
            {
                var tracking = notesContext.Remove(noteImage);
                await notesContext.SaveChangesAsync();
                var isDeleted = tracking.State == EntityState.Deleted;
                NotifyStateChanged();

                return isDeleted;
            }
        }

        public bool AddNotePath(NotePath notePath)
        {
            using (var notesContext = _dbContextFactory.CreateDbContext())
            {
                var tracking = notesContext.Add(notePath);
                notesContext.SaveChanges();
                var isAdded = tracking.State == EntityState.Added;
                NotifyStateChanged();

                return isAdded;
            }
        }

        public async Task<bool> DeleteNotePath(NotePath notePath)
        {
            using (var notesContext = _dbContextFactory.CreateDbContext())
            {
                var tracking = notesContext.Remove(notePath);
                await notesContext.SaveChangesAsync();
                var isDeleted = tracking.State == EntityState.Deleted;
                NotifyStateChanged();

                return isDeleted;
            }
        }



        public void StyleSelectNote(Note note)
        {
            if (note.Selected == false)
            {

                note.Selected = true;
            }
            else
            {
                note.Selected = false;
            }

        }



        #endregion

        #region DropZonePg

        public Dictionary<long, FingerPaintPolyline> inProgressPolylines = new Dictionary<long, FingerPaintPolyline>();
        public List<FingerPaintPolyline> completedPolylines = new List<FingerPaintPolyline>();
        public SKCanvasView? skiaView = null!;

        public SKBitmap? saveBitmap;
        public SKCanvas? paintSKCanvas;
       

       // public bool EditMode = false;

        public bool BitmapDrawed = false;

        public float strokeWidth = 8;
        public SKColor selectedColor;
        // public string defaultColor = "rgb(0,0,0)";
        public string defaultColor = "#000000";
        // public SKColor selectedBackgroundColor;
        //public string defaultBackgroundColor = "rgb(255,222,184)";
        //public string defaultBackgroundColor ;


        public string PointerEvent = "none";
        public string? PenStyle;
        public void EnableDraw()
        {
            paintMode = PaintMode.Pen;
            PointerEvent = "auto";

            if (PenStyle == "transparent")
            {
                selectedColor = SKColor.Parse(defaultColor.Replace("#", "#99"));
                

            }
            else
            {
                selectedColor = SKColor.Parse(defaultColor);

            }
          
           
            NotifyStateChanged();

        }

        public void EnableTransparentColor()
        {
            paintMode = PaintMode.Pen;

            PointerEvent = "auto";
            //selectedColor = selectedColor.WithAlpha(100);
            selectedColor = SKColor.Parse(defaultColor.Replace("#", "#99"));
            NotifyStateChanged();

        }

        public void SetBackgroundColor(string value)
        {

            editNote.BackgroundColor = value;
            NotifyStateChanged();
        }

      
        public void EnableHand()
        {
            paintMode = PaintMode.Drag;
            //PointerEvent = "auto";
            PointerEvent = "none";
            NotifyStateChanged();

        }

       
        public void ErasePaint()
        {
            paintMode = PaintMode.Eraser;
            PointerEvent = "auto";
            //selectedColor = SKColors.Transparent;
            NotifyStateChanged();

        }
        #endregion

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

        public async Task DownloadProjectFile()
        {
            try
            {
                // Create the package directory
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + FilePaths.PackageDirectory);

                // Create the meta object and serialize it to a byte array
                var metaObject = new MetaObject
                {
                    Version = "1.0"
                };
                byte[] metaJsonBytes = SerializeMetaObject(metaObject);

                // Write the serialized meta object to a file
                File.WriteAllBytes(ProjectPath.Parent.FullName + "/"+ FilePaths.MetaFilePath, metaJsonBytes);

                // Serialize the full project object to a file
                Project fullPrjct = await GetFullProject();
                if (fullPrjct != null)
                {
                    await SerializeProjectAsync(fullPrjct);
                }

                // Serialize the list of note cards to a file
                List<NotePacket> allNoteCards = await GetAllNoteCards();
                if (allNoteCards != null)
                {
                    await SerializeNoteCardsAsync(allNoteCards);
                }

                // Create the package file
                var packageFilePath = AppDomain.CurrentDomain.BaseDirectory + FilePaths.PackageDirectory + "/" + FilePaths.PackageFilePath;
                ZipFile.CreateFromDirectory(ProjectPath.Parent.FullName, packageFilePath);

                // Read the package file into a byte array
                fileArray = await File.ReadAllBytesAsync(packageFilePath);

                // Delete the package file
                if (Path.HasExtension(packageFilePath))
                {
                    File.Delete(packageFilePath);
                }
            }
            catch (Exception ex)
            {
                // Log the error or display an error message to the user
            }
        }

        private byte[] SerializeMetaObject(MetaObject metaObject)
        {
            return JsonSerializer.SerializeToUtf8Bytes(metaObject);
        }

        private async Task SerializeProjectAsync(Project project)
        {
            var jsonProjectFilePath = ProjectPath.Parent.FullName + "/" + FilePaths.ProjectFilePath;
            await using (FileStream createProjectStream = File.Create(jsonProjectFilePath))
            {
                await JsonSerializer.SerializeAsync<Project>(createProjectStream, project, ProjectOptions);
            }
        }
        private async Task SerializeNoteCardsAsync(List<NotePacket> noteCards)
        {
            var jsonNoteCardsFilePath = ProjectPath.Parent.FullName + "/" + FilePaths.NoteCardsFilePath;
            await using (FileStream createNotecardsStream = File.Create(jsonNoteCardsFilePath))
            {
                await JsonSerializer.SerializeAsync<List<NotePacket>>(createNotecardsStream, noteCards, NoteCardsOptions);
            }
        }



        #endregion

        #region Collection ViewModel

        
        private bool _showDeleteNCConfirmation;
        public bool showDeleteNCConfirmation
        {
            get { return _showDeleteNCConfirmation; }
            set { this.RaiseAndSetIfChanged(ref _showDeleteNCConfirmation, value); NotifyStateChanged(); }

        }

        public NotesCollection? ContextMenuNotesCollection = null;


        #endregion



        public async Task BuildProject()
        {
            
            if (ProjectPath is null || ProjectPath.Exists == false)
            {
                ProjectPath = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "project/collections");
            }
  

            var jsonProjectFile = ProjectPath.Parent.FullName + "/jsonFile.json";

            using (FileStream openStreamPrj = File.OpenRead(jsonProjectFile))
            {
                Project? newProject =
                  await JsonSerializer.DeserializeAsync<Project>(openStreamPrj);
                MainProject = newProject;
            }


            await InsertProject(MainProject);

            var jsonNoteCardsFile = ProjectPath.Parent.FullName + "/notecards.json";

            using (FileStream openStreamNC = File.OpenRead(jsonNoteCardsFile))
            {
                List<NotePacket>? noteCardsList =
                  await JsonSerializer.DeserializeAsync<List<NotePacket>>(openStreamNC);
                await NewRangNoteCards(noteCardsList);
            }


         // Required   await GetNotes();
            newProjectDialog = false;

        }


        public void NotifyStateChanged() => OnChange?.Invoke();

    }


    public class WindowDimension
    {
        public WindowDimension()
        {

            Width = 1024;
            Height = 800;

        }
        public int Width { get; set; }
        public int Height { get; set; }

        public int ThumbWidth
        {
            get
            {

                return (int)Math.Round((35 * Width) / 100d);
            }
        }
        public int ThumbHeight { 
            get
            {
                return (int)Math.Round((9 * ThumbWidth) / 16d);
            }
                
            }
    }
    
    public enum PaintMode
    {
        Drag,
        Pen,
        Eraser
        
    }

    record MetaObject
    {
        public string Version { get; init; } = "1.0";
    }



}
