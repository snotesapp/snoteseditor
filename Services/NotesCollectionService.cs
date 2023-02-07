using System.Collections.Generic;
using BlazorApp1.Data;
using BlazorApp1.Repositories;

namespace BlazorApp1.Services
{
    public class NotesCollectionService
    {
        private readonly NotesCollectionRepository _repository;

        public NotesCollectionService(NotesCollectionRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<NotesCollection> GetNotesCollections()
        {
            return _repository.GetNotesCollections();
        }

        public async Task AddNotesCollection(NotesCollection notesCollection)
        {
            await _repository.AddNotesCollection(notesCollection);
        }

        public async Task UpdateNotesCollection(NotesCollection notesCollection)
        {
           await _repository.UpdateNotesCollection(notesCollection);
        }

        public async Task DeleteNotesCollection(int id)
        {
           await _repository.DeleteNotesCollection(id);
        }
    }
}
