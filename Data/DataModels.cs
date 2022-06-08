using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace BlazorApp1.Data
{
    public class Project
    {

        [Key]
        public int ProjectID { get; set; }
        public string Name { get; set; }


        public List<Card> Cards { get; set; }


        public List<NotesCollection> NotesCollection { get; set; }

    }

    public class Card
    {

        public int CardID { get; set; }
        public string Title { get; set; }
        public string Resume { get; set; }
        public bool Selected { get; set; }
        public int? ParentID { get; set; }
        public Card Parent { get; set; }
        public int ProjectFK { get; set; }
        public Project Project { get; set; }
        public List<NoteCard> NoteCards { get; set; }


    }

    public class NoteCard
    {

        public int? Order { get; set; }
        public int CardID { get; set; }
        public Card Card { get; set; }

        public int NoteID { get; set; }
        public Note Note { get; set; }
    }

    public class Note
    {

        [Key]
        public int NoteID { get; set; }
        public string Text { get; set; }


        public List<NoteImage> Images { get; set; }

        public int NotesCollectionFK { get; set; }

        public NotesCollection NotesCollection { get; set; }

        public List<NoteCard> NoteCards { get; set; }


    }
    public enum ImgLocationType { Url, Local }

    public class NoteImage
    {
        [Key]
        public int NoteImageID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Location { get; set; }
        public ImgLocationType ImgLocationType { get; set; }

        public int NoteFK { get; set; }
        public Note Note { get; set; }


        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(X, Y, Width, Height);
            }
            set
            {
                X = value.X; Y = value.Y; Width = value.Width; Height = value.Height;
            }
        }

    }

    public class NotesCollection
    {
        
        public int NotesCollectionID { get; set; }
        public string Title { get; set; }
        public bool Selected { get; set; }

        public int ProjectFK { get; set; }
        public Project Project { get; set; }

        public List<Note> Note { get; set; }

    }



}
