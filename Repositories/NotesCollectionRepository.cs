using BlazorApp1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using SkiaSharp;

namespace BlazorApp1.Repositories
{
    public class NotesCollectionRepository
    {
        private readonly IDbContextFactory<SNotesDBContext> _dbContextFactory;
        public NotesCollectionRepository(IDbContextFactory<SNotesDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            using var db = _dbContextFactory.CreateDbContext();
            db.Database.EnsureCreatedAsync();
        }

        public IEnumerable<NotesCollection> GetNotesCollections()
        {
            using (var collectionContext = _dbContextFactory.CreateDbContext())
            {

                return collectionContext.NotesCollection.ToList();

            }

        }

        public async Task  AddNotesCollection(NotesCollection notesCollection)
        {
            using (var collectionContext = _dbContextFactory.CreateDbContext())
            {

              await collectionContext.NotesCollection.AddAsync(notesCollection);
                await collectionContext.SaveChangesAsync();

            }
            
        }

        public async Task UpdateNotesCollection(NotesCollection notesCollection)
        {
            using (var collectionContext = _dbContextFactory.CreateDbContext())
            {

                collectionContext.NotesCollection.Update(notesCollection);
               await collectionContext.SaveChangesAsync();
            }
    
        }

        public async Task DeleteNotesCollection(int id)
        {
            using (var collectionContext = _dbContextFactory.CreateDbContext())
            {

                var notesCollection = await collectionContext.NotesCollection.FindAsync(id);
                if (notesCollection != null)
                {
                    collectionContext.NotesCollection.Remove(notesCollection);
                    await collectionContext.SaveChangesAsync();
                }

            }

           
        }

    }

    }



