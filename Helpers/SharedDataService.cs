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

        private bool _newProjectDialog;
        public bool newProjectDialog
        {
            get { return _newProjectDialog; }
            set { this.RaiseAndSetIfChanged(ref _newProjectDialog, value); NotifyStateChanged(); }

        }

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

      
        #region Collection ViewModel

        
        private bool _showDeleteNCConfirmation;
        public bool showDeleteNCConfirmation
        {
            get { return _showDeleteNCConfirmation; }
            set { this.RaiseAndSetIfChanged(ref _showDeleteNCConfirmation, value); NotifyStateChanged(); }

        }

        public NotesCollection? ContextMenuNotesCollection = null;


        #endregion



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
