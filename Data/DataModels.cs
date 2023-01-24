using Microsoft.JSInterop;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using BlazorComponent;
using BlazorApp1.Helpers;

namespace BlazorApp1.Data
{

    public class Project
    {

        [Key]
        public int ProjectID { get; set; }
        public string Name { get; set; }
        public List<Packet> Packets { get; set; }

        public List<NotesCollection> NotesCollection { get; set; }

    }
    
    public class Packet
    {

        public int PacketID { get; set; }
        public string Title { get; set; }
        public string? Resume { get; set; }
        public bool Selected { get; set; }
        public bool Editable { get; set; }

        public int? ParentID { get; set; }
        [JsonIgnore]
        public Packet? Parent { get; set; }
        public int ProjectFK { get; set; }
        public Project Project { get; set; }
        public List<NotePacket> NotePackets { get; set; }


    }

    public class NotePacket
    {

        public int Order { get; set; }
        public int PacketID { get; set; }
        public Packet? Packet { get; set; }

        public int NoteID { get; set; }
        public Note? Note { get; set; }

        public bool IsDragOver { get; set; }

    }

    

    public class Note
    {

        [Key]
        public int NoteID { get; set; }
        public string Text { get; set; }

        public List<NoteImage> Images { get; set; }
        public int NotesCollectionFK { get; set; }

        public NotesCollection NotesCollection { get; set; }
        public List<NotePacket> NotePackets { get; set; }
        public List<NotePath> NotePaths { get; set; }


        public bool Selected { get; set; }

        public string? MainImg { get; set; }

       // public string? Thumbnail { get; set; }

       
        private string _thumbnail;

        public string? Thumbnail
        {
        get {
                if (MainImg is not null && _thumbnail is null)
                {
                    using (SKImage image = SKImage.FromBitmap(SKBitmap.Decode(MainImg)))
                    {
                       
                        //SKRectI sKRectI = new SKRectI(0, 0,500,281);
                        //SKImage subImage = image.Subset(sKRectI);
                        SKData thdata = image.Encode();
                       _thumbnail = "data:image/jpeg;base64," + Convert.ToBase64String(thdata.ToArray());
                       
                    }

                }
                return _thumbnail; }
           
        }



        public string? BackgroundColor { get; set; } 

        public int MainImgWidth { get; set; }
        public int MainImgHeight { get; set; }

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

        public string ImgContentType { get; set; }

        private string _locaton;
        public string? Location
        {
            get
            {

                if (_locaton is null)
                {

                    //For Reading Files
                    using (FileStream fileStream = File.OpenRead(ImgURI))
                    {
                        byte[] array = new byte[fileStream.Length];

                        fileStream.Read(array, 0, array.Length);
                        _locaton = "data:" + ImgContentType + ";base64," + Convert.ToBase64String(array);
                        //var testurl = await _module.InvokeAsync<string>("imgStreamToSrc", fileStream);
                        // _locaton= js.InvokeAsync<string>("imgStreamToSrc", fileStream).GetAwaiter().GetResult();
                    }
                }
                return _locaton;


            }
            
        }
       
       // public string? Location { get; set; }
        public string ImgURI { get; set; }

        public ImgLocationType ImgLocationType { get; set; }

        public int NoteFK { get; set; }
        public Note Note { get; set; }

        public bool Selected { get; set; }
         [JsonIgnore]
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

    public class NotePath
    {
        [Key]
        public int PathID { get; set; }
        public string SvgPath { get; set; }
        public string StrokeColor { set; get; }
        public int StrokeWidth { set; get; }
        public string StrokeBlendMode { set; get; }

        public int NoteFK { get; set; }
        public Note Note { get; set; }
    }


    public class NotesCollection
    {


        public int NotesCollectionID { get; set; }
        public string Title { get; set; }
        public bool Selected { get; set; }
        public bool Editable { get; set; }

        public int ProjectFK { get; set; }
        public Project Project { get; set; }

        public List<Note> Note { get; set; }

      

    }




}

