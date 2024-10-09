using BlazorApp1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using SkiaSharp;
using SqliteWasmHelper;

namespace BlazorApp1.Repositories
{
    public class NotesCollectionRepository
    {
        private readonly ISqliteWasmDbContextFactory<SNotesDBContext> _dbContextFactory;
        public NotesCollectionRepository(ISqliteWasmDbContextFactory<SNotesDBContext> dbContextFactory)
        {
           
            _dbContextFactory = dbContextFactory;
            //using var db = _dbContextFactory.CreateDbContext();
            //db.Database.EnsureCreatedAsync();
        }

        public async Task<IEnumerable<NotesCollection>> GetNotesCollections()
        {
            using (var collectionContext = await _dbContextFactory.CreateDbContextAsync())
            {

                return collectionContext.NotesCollection.ToList();

            }

        }

        public async Task  AddNotesCollection(NotesCollection notesCollection)
        {
            using (var collectionContext = await _dbContextFactory.CreateDbContextAsync())
            {

              await collectionContext.NotesCollection.AddAsync(notesCollection);
                await collectionContext.SaveChangesAsync();

            }
            
        }

        public async Task UpdateNotesCollection(NotesCollection notesCollection)
        {
            using (var collectionContext = await _dbContextFactory.CreateDbContextAsync())
            {

                collectionContext.NotesCollection.Update(notesCollection);
               await collectionContext.SaveChangesAsync();
            }
    
        }

        public async Task DeleteNotesCollection(int id)
        {
            using (var collectionContext = await _dbContextFactory.CreateDbContextAsync())
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



