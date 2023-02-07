using BlazorApp1.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services
{
    public class NoteService
    {


        private readonly IDbContextFactory<SNotesDBContext> _dbContextFactory;
        public NoteService(IDbContextFactory<SNotesDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            using var db = _dbContextFactory.CreateDbContext();
            db.Database.EnsureCreatedAsync();
        }

        public async Task<List<Note>> GetNotes()
        {
            using (var noteContext = _dbContextFactory.CreateDbContext())
            {
                List<Note> notes= await noteContext.Note.Where(nc => nc.NotesCollection.Selected).Take(20).ToListAsync();

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
                using (var notesContext = _dbContextFactory.CreateDbContext())
                {
                    var selectedNotes = await notesContext.Note
                        .Where(nc => nc.NotesCollection.Selected == true)
                        .Skip(pageIndex)
                        .Take(pageSize)
                        .ToListAsync();

                   
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
                List<Note> notes = await notesContext.Note.Where(nt => nt.NotesCollection.Selected == true && nt.Text.ToLower().Contains(NotesTextFilter.Trim().ToLower())).Take(20).ToListAsync();
                
                return notes;

            }
        }


        public async Task<Note> GetNote(Note note)
        {
            using (var notesContext = _dbContextFactory.CreateDbContext())
            {
                Note newnote   = await notesContext.Note.Where(nt => nt.NoteID == note.NoteID).Include(im => im.Images).Include(np => np.NotePaths).FirstOrDefaultAsync();
               
                return newnote;

            }
        }


        public async Task AddNote(Note note)
        {
            using (var noteContext = _dbContextFactory.CreateDbContext())
            {

                await noteContext.Note.AddAsync(note);
                await noteContext.SaveChangesAsync();

            }

        }

        public async Task UpdateNote(Note note)
        {
            using (var noteContext = _dbContextFactory.CreateDbContext())
            {

                noteContext.Note.Update(note);
                await noteContext.SaveChangesAsync();
            }

        }


        public async Task DeleteNote(int id)
        {
            using (var noteContext = _dbContextFactory.CreateDbContext())
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
            using (var notesContext = _dbContextFactory.CreateDbContext())
            {
                notesContext.RemoveRange(note.NotePaths);
                await notesContext.SaveChangesAsync();

               
            }
        }

        public async Task DeleteNoteImg(NoteImage noteImage)
        {
            using (var notesContext = _dbContextFactory.CreateDbContext())
            {
                notesContext.Remove(noteImage);
                await notesContext.SaveChangesAsync();

            }
        }

        public async Task DeleteNotePath(NotePath notePath)
        {
            using (var notesContext = _dbContextFactory.CreateDbContext())
            {
                notesContext.Remove(notePath);
                await notesContext.SaveChangesAsync();

            }
        }


    }
}
