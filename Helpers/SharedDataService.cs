using BlazorApp1.Data;
using BlazorApp1.Pages.Components;
using BlazorComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using OneOf.Types;
using ReactiveUI;
using SkiaSharp;
using SkiaSharp.Views.Blazor;
using System.Data;
using System.Globalization;
using System.IO.Compression;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        #endregion




        public PaintMode paintMode = PaintMode.Drag;

        //Note Props
        public Note editNote = new Note()
        {
            Text = "",
            Images = new List<NoteImage>(),
            NotePaths = new List<NotePath>(),
            BackgroundColor = "#FB7B5C"

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
            Cards = new List<Card>(),
            NotesCollection = new List<NotesCollection>() ,
        };
        public DirectoryInfo ProjectPath { get; set; }


        public NotesCollection AddNotesSelectedNC;

        public Card? SelectedCard;

        public List<Card> AllCards;
        public List<Card> SelectionCards = new();
        public List<Card> ChildCards = new();

        public WindowDimension Wdimension = new();
        public NoteCard? SelectedNoteCard;


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

            }

            return true;
        }

        public async Task<Project> GetProject()
        {
            using (var projectsContext = _dbContextFactory.CreateDbContext())
            {
                Project  mProject = await projectsContext.Projects.Include(nc => nc.NotesCollection).Include(cr => cr.Cards.Where(s => s.Selected == true)).ThenInclude(pc => pc.Parent).FirstOrDefaultAsync();
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
                    .Include(cr => cr.Cards).FirstOrDefaultAsync();
               // return await projectsContext.Projects.Include(nc => nc.NotesCollection).Include(cr => cr.Cards).FirstOrDefaultAsync();

            }
        }

        #endregion

        #region NCollections Crud
        public async Task<bool> AddCollection(NotesCollection newNC)
        {
            using (var collectionContext = _dbContextFactory.CreateDbContext())
            {

                collectionContext.NotesCollection.Add(newNC);
                await collectionContext.SaveChangesAsync();
                MainProject.NotesCollection.Add(newNC);

            }
            NotifyStateChanged();
            return true;

        }


        public async Task<bool> UpdateNCollection(NotesCollection updateNC)
        {
            using (var collectionContext = _dbContextFactory.CreateDbContext())
            {
                var tracking = collectionContext.Update(updateNC);
                await collectionContext.SaveChangesAsync();

                var isModified = tracking.State == EntityState.Modified;
                return isModified;
            }
        }


        public async Task<bool> DeleteNCollection(NotesCollection deleteNC)
        {
            using (var collectionContext = _dbContextFactory.CreateDbContext())
            {
                var tracking = collectionContext.Remove(deleteNC);
                await collectionContext.SaveChangesAsync();
                var isDeleted = tracking.State == EntityState.Deleted;
                return isDeleted;
            }
        }


        #endregion

        #region Cards Crud
        public async Task<bool> AddCard(Card newCard)
        {
            using (var cardsContext = _dbContextFactory.CreateDbContext())
            {

                // cardsContext.Add(newCard);
                cardsContext.Cards.Add(newCard);
                await cardsContext.SaveChangesAsync();
                MainProject = await GetProject();
            //    MainProject.Cards.Add(newCard);

            }
            NotifyStateChanged();
            return true;

        }

        public async Task<Card> GetCard(Card card)
        {
            using (var cardsContext = _dbContextFactory.CreateDbContext())
            {
               
              SelectedCard =  await cardsContext.Cards.Where(c => c.CardID == card.CardID)
                    .Include(nd => nd.NoteCards.OrderBy(od => od.Order)).ThenInclude(nt => nt.Note).ThenInclude(im => im.Images)
                    .Include(nd => nd.NoteCards.OrderBy(od => od.Order)).ThenInclude(nt => nt.Note).ThenInclude(pth => pth.NotePaths)
                    .Include(p => p.Parent).FirstOrDefaultAsync();

                SelectedNoteCard = SelectedCard?.NoteCards.Count == 0 ? null : SelectedNoteCard;



                ChildCards = await GetChildCards(card);
                NotifyStateChanged();
                return SelectedCard;

            }
        }

        public async Task<Card> SetInitialeNC()
        {
            using (var cardsContext = _dbContextFactory.CreateDbContext())
            {
                try{
                    SelectedCard = await cardsContext.Cards.Where(c => c.CardID == 1)
                      .Include(nd => nd.NoteCards.OrderBy(od => od.Order)).ThenInclude(nt => nt.Note).ThenInclude(im => im.Images)
                      .Include(nd => nd.NoteCards.OrderBy(od => od.Order)).ThenInclude(nt => nt.Note).ThenInclude(pth => pth.NotePaths)
                      .Include(p => p.Parent).FirstOrDefaultAsync();
                    ChildCards = await GetChildCards(SelectedCard);
                    SelectedNoteCard = SelectedCard.NoteCards.FirstOrDefault();
                    SwitchMenus("notecards");

                    NotifyStateChanged();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Can't set the first card");
                }
                
                return SelectedCard;

            }
        }



        public async Task<List<Card>> GetCards()
        {
            using (var cardsContext = _dbContextFactory.CreateDbContext())
            {
                // return await cardsContext.Cards.Where(sl => sl.Selected == true ).Include(pj => pj.Project).Include(pr => pr.Parent).Include(ci => ci.NoteCards).ThenInclude(sn => sn.Note).ThenInclude(im => im.NotesCollection).Include(ci => ci.NoteCards).ThenInclude(sn => sn.Note).ThenInclude(im => im.Images).ToListAsync();

                return await cardsContext.Cards.Where(sl => sl.Selected == true ).ToListAsync();
            }
        }

        public async Task<List<Card>> GetSelectionCards(Card card)
        {
            using (var cardsContext = _dbContextFactory.CreateDbContext())
            {

                List<Card> chcards = new List<Card> { card };
                int i = 0;
                do
                {
                    var templist = await GetChildCards(chcards[i]);
                    if (templist.Count > 0)
                    {
                        chcards.AddRange(templist);
                    }

                    i++;
                } while (i < chcards.Count);



                Task<List<Card>> selcard = cardsContext.Cards.Where(p => !chcards.Select(p2 => p2.CardID).Contains(p.CardID)).OrderBy(p => p.Title).ToListAsync();
                return await selcard;
            }
        }

        public async Task<List<Card>> GetChildCards(Card card)
        {
            using (var cardsContext = _dbContextFactory.CreateDbContext())
            {
                return await cardsContext.Cards.Where(c => c.ParentID == card.CardID).ToListAsync();
                

            }
        }


        public async Task<bool> UpdateCard(Card card)
        {
            using (var cardsContext = _dbContextFactory.CreateDbContext())
            {
                var tracking = cardsContext.Cards.Update(card);

                await cardsContext.SaveChangesAsync();
                var isModified = tracking.State == EntityState.Modified;
                NotifyStateChanged();
                return isModified;
            }
        }

        public async Task<bool> DeleteCard(Card Card)
        {
            using (var cardsContext = _dbContextFactory.CreateDbContext())
            {              
                var tracking = cardsContext.Remove(Card);
                await cardsContext.SaveChangesAsync();
                var isDeleted = tracking.State == EntityState.Deleted;
                return isDeleted;
            }
        }




        #endregion

        #region NoteCards Crud

        public async Task<NoteCard> GetNoteCard(NoteCard noteCard)
        {
            using (var notecardsContext = _dbContextFactory.CreateDbContext())
            {
                NotifyStateChanged();
                return await notecardsContext.NoteCards.Where(ncd => ncd.NoteID == noteCard.NoteID && ncd.CardID == noteCard.CardID).Include(nt => nt.Note).ThenInclude(im => im.Images).FirstOrDefaultAsync();
            }
        }

        public async Task <List<NoteCard>> GetAllNoteCards()
        {
            using (var notecardsContext = _dbContextFactory.CreateDbContext())
            {
                return await notecardsContext.NoteCards.ToListAsync();

            }
        }

        public async Task<bool> NewNoteCard(Card addCard, Note addNote)
        {
            using (var notecardsContext = _dbContextFactory.CreateDbContext())
            {
                //cardsContext.Note.Add(NewNote);
                //await cardsContext.SaveChangesAsync();
                // cardsContext.NoteCards.Add(new NoteCard { Card =NewCard , Note = NewNote, Order = NewCard.NoteCards.Count });
                NoteCard noteCard = new NoteCard() { CardID = addCard.CardID, NoteID = addNote.NoteID };

                try
                {
                    if (!notecardsContext.NoteCards.Contains(noteCard))
                    {
                        notecardsContext.NoteCards.Add(new NoteCard { CardID = addCard.CardID, NoteID = addNote.NoteID });
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
                NoteCard noteCard = new NoteCard() { CardID = cardID, NoteID = noteID };

                try
                {
                    if (!notecardsContext.NoteCards.Contains(noteCard))
                    {
                        notecardsContext.NoteCards.Add(new NoteCard() { CardID = cardID, NoteID = noteID });
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


        public async Task<bool> AddRangNoteCard(Card addCard, List<Note> addNotes)
        {
            using (var notecardsContext = _dbContextFactory.CreateDbContext())
            {
                foreach (Note addNote in addNotes)
                {
                    NoteCard noteCard = new NoteCard() { CardID = addCard.CardID, NoteID = addNote.NoteID };

                    try
                    {
                        if (!notecardsContext.NoteCards.Contains(noteCard))
                        {
                            notecardsContext.NoteCards.Add(new NoteCard { CardID = addCard.CardID, NoteID = addNote.NoteID });
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

        public async Task<bool> NewRangNoteCards(List<NoteCard> noteCards)
        {
            using (var notecardsContext = _dbContextFactory.CreateDbContext())
            {

                await notecardsContext.AddRangeAsync(noteCards);
                await notecardsContext.SaveChangesAsync();


            }
            return true;
        }

        public async Task<bool> RemoveNoteCard(NoteCard noteCards)
        {
            using (var notecardsContext = _dbContextFactory.CreateDbContext())
            {

                notecardsContext.NoteCards.Remove(noteCards);
                await notecardsContext.SaveChangesAsync();
                NotifyStateChanged();

            }
            return true;
        }

       

        #endregion

        #region Notes Crud

        public async Task<bool> SaveNote()
        {

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
                Images = new List<NoteImage>(),
                NotePaths = new List<NotePath>(),
                BackgroundColor = "#FB7B5C",

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

        public async Task<List<Note>> GetNotes()
        {
            using (var notesContext = _dbContextFactory.CreateDbContext())
            {
                selectedNCNotes = await notesContext.Note.Where(nc => nc.NotesCollection.Selected == true).ToListAsync();
                NotifyStateChanged();
                return selectedNCNotes;

            }
        }

        public async Task<List<Note>> GetNotes(string NotesTextFilter)
        {
            using (var notesContext = _dbContextFactory.CreateDbContext())
            {
                selectedNCNotes = await notesContext.Note.Where(nt => nt.NotesCollection.Selected == true && nt.Text.Contains(NotesTextFilter)).ToListAsync();
                NotifyStateChanged();
                return selectedNCNotes;

            }
        }

        public async Task<Note> GetNote(Note note)
        {
            using (var notesContext = _dbContextFactory.CreateDbContext())
            {
                editNote = await notesContext.Note.Where(nt => nt.NoteID == note.NoteID).Include(im =>im.Images).Include(np => np.NotePaths).FirstOrDefaultAsync();
                NotifyStateChanged();
                return editNote;

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


        public async Task ClearCanvas()
        {
            if (editNote.NoteID == 0)
            {
                completedPolylines.Clear();
                editNote.NotePaths.Clear();
            }
            else
            {
                completedPolylines.Clear();
                editNote.NotePaths = editNote.NotePaths.Where(nt => nt.PathID != 0).ToList();
                await DeleteNotePaths(editNote);
            }
           
           
            //   inProgressPolylines.Clear();
            saveBitmap = new SKBitmap(Wdimension.Width, Wdimension.Height);

            skiaView.Invalidate();

            NotifyStateChanged();
            // StateHasChanged();
        }

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

        public byte[] fileArray;
        public async Task DownloadProjectFile()
        {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Package");


            Project fullPrjct = await GetFullProject();
            JsonSerializerOptions projectoptions = new()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true,
                IgnoreReadOnlyProperties = true,
            };

            List<NoteCard> allNoteCards = await GetAllNoteCards();
            JsonSerializerOptions notecardsoptions = new()
            {
                WriteIndented = true,
            };

            /*
            string jsonString = JsonSerializer.Serialize<Project>(fullPrjct, options);



            var test1FilePath = AppDomain.CurrentDomain.BaseDirectory + "PackageTest/jsonFile.json";

            using (StreamWriter? sw = new StreamWriter(test1FilePath))
            {
                sw.WriteLine(jsonString);
            }
            */

            //  var test1FilePath = AppDomain.CurrentDomain.BaseDirectory + "PackageTest/jsonFile.json";

            var jsonProjectFilePath = ProjectPath.Parent.FullName + "/jsonFile.json";
            using FileStream createProjectStream = File.Create(jsonProjectFilePath);
            await JsonSerializer.SerializeAsync<Project>(createProjectStream, fullPrjct, projectoptions);
            await createProjectStream.DisposeAsync();

            var jsonNoteCardsFilePath = ProjectPath.Parent.FullName  + "/notecards.json";
            using FileStream createNotecardsStream = File.Create(jsonNoteCardsFilePath);
            await JsonSerializer.SerializeAsync<List<NoteCard>>(createNotecardsStream, allNoteCards, notecardsoptions);
            await createNotecardsStream.DisposeAsync();



            var packageFilePath = AppDomain.CurrentDomain.BaseDirectory + "Package/package.zip";

            ZipFile.CreateFromDirectory(ProjectPath.Parent.FullName, packageFilePath);
            
            /*
            var filePackage = new FilePackage
            {
                FilePath = packageFilePath,
                ContentFilePathList = new List<string>
                {
                     test1FilePath, test2FilePath
                }
            };

            var filePackageWriter = new FilePackageWriter(filePackage);
            filePackageWriter.GeneratePackage(true);
            */

            fileArray = await File.ReadAllBytesAsync(packageFilePath);
            File.Delete(packageFilePath);

            /*
            Project fullPrjct = await GetFuProject();
            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true
            };
            string jsonString = JsonSerializer.Serialize<Project>(fullPrjct,options);

            var test1FilePath = AppDomain.CurrentDomain.BaseDirectory + "test_1.txt";

            using (StreamWriter? sw = new StreamWriter(test1FilePath))
            {
                sw.WriteLine(jsonString);
            }


            var packageDirectory = AppDomain.CurrentDomain.BaseDirectory + "packageDirectory";
            Directory.CreateDirectory(packageDirectory);
            File.Copy(test1FilePath, packageDirectory+"/"+"file.txt", true);
            if (File.Exists("package.zip"))
            {
                File.Delete("package.zip");
            }
            ZipFile.CreateFromDirectory(packageDirectory, "package.zip");

            fileArray = await File.ReadAllBytesAsync(AppDomain.CurrentDomain.BaseDirectory + "package.zip");
            var listfiles = Directory.GetFiles(packageDirectory);
           // var readcontent = File.ReadAllText(test1FilePath);  

           // fileArray = System.Text.Encoding.UTF8.GetBytes(jsonString) ;

            Project? desPrjoject =
               JsonSerializer.Deserialize<Project>(jsonString);

    
         */

        }


        public async Task GetThumbUrl(IJSObjectReference _jsModule,Note note)
        {

            if (note.MainImg is not null)
            {
                using (SKImage image = SKImage.FromBitmap(SKBitmap.Decode(note.MainImg)))
                {
                    SKRectI sKRectI = new SKRectI(0, 0, 500, 500);
                    SKImage subImage = image.Subset(sKRectI);
                    SKData thdata = subImage.Encode();
                    //_thumbnail = "data:image/jpeg;base64," + Convert.ToBase64String(thdata.ToArray());
                    var mStream = new MemoryStream();
                    thdata.SaveTo(mStream);
                    DotNetStreamReference dotnetImageStream = new DotNetStreamReference(mStream);
                   // note.Thumbnail = await _jsModule.InvokeAsync<string>("imgStreamToSrc", dotnetImageStream);

                }
                NotifyStateChanged();
            }


            //await _jsModule.InvokeVoidAsync("showPrompt", "JS function called from .NET");

        }

        private void NotifyStateChanged() => OnChange?.Invoke();

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
  
    #region dbtest
    /*
    
    namespace BlazorSQLiteWasm.Pages;

using Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public partial class Demo
{
  public const string SqliteDbFilename = "DemoData.db";

  private string _version = "unknown";

  private readonly List<Car> _cars = new();

  [Inject]
  private IJSRuntime _js { get; set; }

  [Inject]
  private IDbContextFactory<ClientSideDbContext> _dbContextFactory { get; set; }

  protected override async Task OnInitializedAsync()
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("browser")))
    {
      // create SQLite database file in browser
     // var module = await _js.InvokeAsync<IJSObjectReference>("import", "./dbstorage.js");
     // await module.InvokeVoidAsync("synchronizeFileWithIndexedDb", SqliteDbFilename);
    }

    await using var db = await _dbContextFactory.CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();

    // create seed data
    if (!db.Cars.Any())
    {
      var cars = new[]
      {
        new Car { Brand = "Audi", Price = 21000 },
        new Car { Brand = "Volvo", Price = 11000 },
        new Car { Brand = "Range Rover", Price = 135000 },
        new Car { Brand = "Ford", Price = 8995 }
      };

      await db.Cars.AddRangeAsync(cars);
    }

    await Update(db);

    await base.OnInitializedAsync();
  }

  private async Task SQLiteVersion()
  {
    //await using var db = new SqliteConnection($"Data Source={SqliteDbFilename}");
   // await db.OpenAsync();
   // await using var cmd = new SqliteCommand("SELECT SQLITE_VERSION()", db);
   // _version = (await cmd.ExecuteScalarAsync())?.ToString();
  }

  private async Task Create(Car upCar)
  {
    var db = await _dbContextFactory.CreateDbContextAsync();
    await db.Cars.AddAsync(upCar);
    await Update(db);
  }

  private async Task Update(Car upCar)
  {
    var db = await _dbContextFactory.CreateDbContextAsync();
    var car = await db.Cars.FindAsync(upCar.Id);
    car.Brand = upCar.Brand;
    car.Price = upCar.Price;
    db.Cars.Update(car);
    await Update(db);
  }

  private async Task Delete(int id)
  {
    var db = await _dbContextFactory.CreateDbContextAsync();
    var car = await db.Cars.FindAsync(id);
    db.Cars.Remove(car);
    await Update(db);
  }

  private async Task Update(ClientSideDbContext db)
  {
    await db.SaveChangesAsync();
    _cars.Clear();
    _cars.AddRange(db.Cars);
    StateHasChanged();
  }
}


     */
    #endregion
}
