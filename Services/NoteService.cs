using BlazorApp1.Data;
using Microsoft.EntityFrameworkCore;
using SqliteWasmHelper;

namespace BlazorApp1.Services
{
    public class NoteService
    {


        private readonly ISqliteWasmDbContextFactory<SNotesDBContext> _dbContextFactory;
        public NoteService(ISqliteWasmDbContextFactory<SNotesDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            //using var db =  _dbContextFactory.CreateDbContextAsync();
            //db.Database.EnsureCreatedAsync();
        }

        public async Task<List<Note>> GetNotes()
        {
            using (var noteContext = await _dbContextFactory.CreateDbContextAsync())
            {
               var selectedNotes = noteContext.Note
                    .Where(nc => nc.NotesCollection.Selected)
                    .OrderByDescending(ord => ord.NoteID)
                    .Take(20);
                List<Note> notes = await selectedNotes.ToListAsync();
                return notes;

            }

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
                using (var notesContext = await _dbContextFactory.CreateDbContextAsync())
                {
                    var selectedNotes =  notesContext.Note
                        .Where(nc => nc.NotesCollection.Selected == true)
                        .OrderByDescending(ord => ord.NoteID)
                        .Skip(pageIndex)
                        .Take(pageSize);
                    List<Note> notes = await selectedNotes.ToListAsync();
                    return notes;

                    
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while fetching notes.", ex);
            }
        }


        public async Task<List<Note>> GetNotes(string NotesTextFilter)
        {

            using (var notesContext = await _dbContextFactory.CreateDbContextAsync())
            {
               var selectedNotes =  notesContext.Note
                    .Where(nt => nt.NotesCollection.Selected == true && nt.Text.ToLower().Contains(NotesTextFilter.Trim().ToLower()))
                    .OrderByDescending(ord => ord.NoteID);
                List<Note> notes = await selectedNotes.ToListAsync();
                return notes;

            }
        }


        public async Task<Note> GetNote(Note note)
        {
            using (var notesContext = await _dbContextFactory.CreateDbContextAsync())
            {
                Note newnote   = await notesContext.Note.Where(nt => nt.NoteID == note.NoteID).Include(im => im.Images).Include(np => np.NotePaths).FirstOrDefaultAsync();    
                return newnote;

            }
        }


        public async Task AddNote(Note note)
        {
            using (var noteContext = await _dbContextFactory.CreateDbContextAsync())
            {

                await noteContext.Note.AddAsync(note);
                await noteContext.SaveChangesAsync();

            }

        }

        public async Task UpdateNote(Note note)
        {
            using (var noteContext = await _dbContextFactory.CreateDbContextAsync())
            {

                noteContext.Note.Update(note);
                await noteContext.SaveChangesAsync();
            }

        }


        public async Task DeleteNote(int id)
        {
            using (var noteContext = await _dbContextFactory.CreateDbContextAsync())
            {

                var Note = await noteContext.Note.FindAsync(id);
                if (Note != null)
                {
                    noteContext.Note.Remove(Note);
                    await noteContext.SaveChangesAsync();
                }

            }


        }


        public async Task DeleteNotePaths(Note note)
        {
            using (var notesContext = await _dbContextFactory.CreateDbContextAsync())
            {
                notesContext.RemoveRange(note.NotePaths);
                await notesContext.SaveChangesAsync();

               
            }
        }

        public async Task DeleteNoteImg(NoteImage noteImage)
        {
            using (var notesContext = await _dbContextFactory.CreateDbContextAsync())
            {
                notesContext.Remove(noteImage);
                await notesContext.SaveChangesAsync();

            }
        }

        public async Task DeleteNotePath(NotePath notePath)
        {
            using (var notesContext = await _dbContextFactory.CreateDbContextAsync())
            {
                notesContext.Remove(notePath);
                await notesContext.SaveChangesAsync();

            }
        }


    }
}
