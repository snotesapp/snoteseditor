using BlazorApp1.Data;
using BlazorApp1.Helpers;
using BlazorApp1.Services;
using BlazorComponent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using SkiaSharp;
using SkiaSharp.Views.Blazor;
using System.Data;

namespace BlazorApp1.ViewModels
{
    public class NoteViewModel
    {
        private SharedDataService SharedDataService_service;
        private NoteService NoteService_service;
        public NoteViewModel(NoteService noteService_service,SharedDataService sharedDataService_service)
        {
            NoteService_service = noteService_service;
            SharedDataService_service = sharedDataService_service;
        }

        public void AddNCNotes(NotesCollection notesCollection)
        {

            SharedDataService_service.AddNotesSelectedNC = notesCollection;
            SharedDataService_service.editNote = new Note()
            {
                NotesCollectionFK = notesCollection.NotesCollectionID,
                Images = new List<NoteImage>(),
                NotePaths = new List<NotePath>(),
                BackgroundColor = SharedDataService_service.noteBackgroundColor,
            };

            SharedDataService_service.SwitchMenus("notetools");
           

        }
        public async Task SaveNote()
        {
            SharedDataService_service.noteBackgroundColor = SharedDataService_service.editNote.BackgroundColor;
            SharedDataService_service.editNote.NotesCollectionFK = SharedDataService_service.AddNotesSelectedNC.NotesCollectionID;

            if (!string.IsNullOrWhiteSpace(SharedDataService_service.editNote.Text))
            {
               await NoteService_service.AddNote(SharedDataService_service.editNote);
               

            }

          
            SharedDataService_service.editNote = new Note()
            {
                NotesCollectionFK = SharedDataService_service.AddNotesSelectedNC.NotesCollectionID,
                Images = new List<NoteImage>(),
                NotePaths = new List<NotePath>(),
                BackgroundColor = SharedDataService_service.noteBackgroundColor,
                MainImgWidth = SharedDataService_service.Wdimension.Width,
                MainImgHeight = SharedDataService_service.Wdimension.Height
            };
            SharedDataService_service.saveBitmap = new SKBitmap(SharedDataService_service.Wdimension.Width, SharedDataService_service.Wdimension.Height);
            // await GetNotes();
            SharedDataService_service.NotifyStateChanged();

            
        }

        public async Task UpdateNote()
        {
            await NoteService_service.UpdateNote(SharedDataService_service.editNote);


            SharedDataService_service.NotifyStateChanged();


        }

        public async Task GetNotes()
        {
            SharedDataService_service.selectedNCNotes = await NoteService_service.GetNotes();
            SharedDataService_service.NotifyStateChanged();

        }

        public async Task<List<Note>> GetNotes(int pageIndex = 0, int pageSize = 20)
        {

           List<Note> listnotes = await NoteService_service.GetNotes(pageIndex, pageSize);
            SharedDataService_service.NotifyStateChanged();
            return listnotes;
        
        }

        public async Task<List<Note>> GetNotes(string NotesTextFilter)
        {

            List<Note> listnotes = await NoteService_service.GetNotes(NotesTextFilter);
            SharedDataService_service.selectedNCNotes = listnotes;
            SharedDataService_service.NotifyStateChanged();
            return listnotes;
            
        }

        public async Task<Note> GetNote(Note note)
        {
            Note newnote = await NoteService_service.GetNote(note);
            SharedDataService_service.editNote = newnote;
            SharedDataService_service.NotifyStateChanged();
            return newnote;

        }

        public async Task DeleteNote(Note note)
        {
            await NoteService_service.DeleteNote(note.NoteID);
            SharedDataService_service.NotifyStateChanged();
                 
        }

        public async Task DeleteNoteImgFiles(Note note, bool notSavedImageOnly)
        {
            if (notSavedImageOnly)
            {
                var imgURIs = note.Images.Where(im => im.NoteImageID == 0).Select(image => image.ImgURI).ToArray();
                Parallel.ForEach(imgURIs, imgURI =>
                {
                    if (File.Exists(imgURI))
                    {
                        try
                        {
                            File.Delete(imgURI);
                        }
                        catch (IOException ex)
                        {
                            Console.WriteLine("File Not Found");
                        }
                    }
                });

            }
            else
            {
                Note DeleteNote = await NoteService_service.GetNote(note);
                var imgURIs = DeleteNote.Images.Select(image => image.ImgURI).ToArray();


                Parallel.ForEach(imgURIs, imgURI =>
                {
                    if (File.Exists(imgURI))
                    {
                        try
                        {
                            File.Delete(imgURI);
                        }
                        catch (IOException ex)
                        {
                            Console.WriteLine("File Not Found");
                        }
                    }
                });
            }


        }

        public async Task DeleteNotePaths(Note note)
        {
            await NoteService_service.DeleteNotePaths(note);
            SharedDataService_service.NotifyStateChanged();
           

        }

        public async Task DeleteNoteImg(NoteImage noteImage)
        {
            await NoteService_service.DeleteNoteImg(noteImage);
            SharedDataService_service.NotifyStateChanged();

        }

        public async Task DeleteNotePath(NotePath notePath)
        {
            await NoteService_service.DeleteNotePath(notePath);
            SharedDataService_service.NotifyStateChanged();

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

        public async Task ClearCanvas()
        {
            if (SharedDataService_service.editNote.NoteID == 0)
            {
                SharedDataService_service.completedPolylines.Clear();
                SharedDataService_service.editNote.NotePaths.Clear();
            }
            else
            {
                SharedDataService_service.completedPolylines.Clear();
                SharedDataService_service.editNote.NotePaths = SharedDataService_service.editNote.NotePaths.Where(nt => nt.PathID != 0).ToList();
                await DeleteNotePaths(SharedDataService_service.editNote);
            }


            //   inProgressPolylines.Clear();
            SharedDataService_service.saveBitmap = new SKBitmap(SharedDataService_service.Wdimension.Width, SharedDataService_service.Wdimension.Height);

           SharedDataService_service.skiaView.Invalidate();

           SharedDataService_service.NotifyStateChanged();
            // StateHasChanged();
        }

    }
}
