using BlazorApp1.Data;
using BlazorApp1.Helpers;
using BlazorApp1.Services;
using BlazorBootstrap;
using DynamicData;
using System.Collections;
using System.Data;

namespace BlazorApp1.ViewModels
{
    public class NotesCollectionViewModel
    {
        private readonly NotesCollectionService _service;
        private readonly SharedDataService _dataSvs;
        private readonly NoteViewModel NoteVM;
        private readonly ProjectViewModel ProjectVM;

       

        public NotesCollectionViewModel() { }

        public NotesCollectionViewModel(NotesCollectionService service, SharedDataService dataSvs, NoteViewModel noteVM, ProjectViewModel projectVM)
        {
            
            _service = service;
            _dataSvs = dataSvs;
            NoteVM = noteVM;
            ProjectVM = projectVM;
           
        }


        public async Task AddNotesCollection(string textValue)
        {
            NotesCollection notesCollection = new NotesCollection { Title = textValue, Selected = false, ProjectFK = _dataSvs.MainProject.ProjectID, Note = new List<Note>() };


            await _service.AddNotesCollection(notesCollection);

            _dataSvs.MainProject.NotesCollection.Add(notesCollection);
        }
        public async Task AddNotesCollection(NotesCollection notesCollection)
        {
           
            await _service.AddNotesCollection(notesCollection);

            _dataSvs.MainProject.NotesCollection.Add(notesCollection);
        }

        public async Task UpdateNotesCollection(NotesCollection notesCollection)
        {
           await _service.UpdateNotesCollection(notesCollection);
        }

        public async Task DeleteNotesCollection(int id)
        {
           await _service.DeleteNotesCollection(id);
        }

        public async Task DeleteNotesCollection(NotesCollection notesCollection)
        {
            try
            {

                var filePath = Path.Combine(_dataSvs.ProjectPath.FullName, notesCollection.NotesCollectionID.ToString());
                if (Directory.Exists(filePath))
                {
                    Directory.Delete(filePath, true);

                }
                
                await _service.DeleteNotesCollection(notesCollection.NotesCollectionID);

                _dataSvs.MainProject = await ProjectVM.GetProject();
                await NoteVM.GetNotes();
                _dataSvs.showDeleteNCConfirmation = false;
            }
            catch (Exception ex)
            {
                // log the error
                Console.WriteLine("Error deleting notes collection ");
            }
        }


        public event Action OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();


    }
}
