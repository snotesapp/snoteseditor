using SkiaSharp;
using System.Drawing;

namespace BlazorApp1.Helpers
{
    public class FingerPaintPolyline
    {
        public FingerPaintPolyline()
        {
            Path = new SKPath();
        }

        public SKPath Path { set; get; }

        public SKColor StrokeColor { set; get; }

        public float StrokeWidth { set; get; }

        public SKBlendMode StrokeBlendMode { set; get; }    
    }
}
