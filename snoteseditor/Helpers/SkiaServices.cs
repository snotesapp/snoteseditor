using SkiaSharp;
using System.Data;

namespace BlazorApp1.Helpers
{
    public class SkiaServices
    {

        private SharedDataService dataSvs;
        public SkiaServices(SharedDataService dataSvs)
        {
            this.dataSvs = dataSvs;

        }
    
        public void CreateThumbImg(string CollectionId)
        {
            string bitmapPath;
            if (dataSvs.editNote.MainImg is not null)
            {

                bitmapPath = dataSvs.editNote.MainImg;
                using (SKImage image = SKImage.FromBitmap(dataSvs.saveBitmap))
                {

                    SKRectI sKRectI = new SKRectI(0, 0, dataSvs.Wdimension.ThumbWidth, dataSvs.Wdimension.ThumbHeight);
                    SKImage subImage = image.Subset(sKRectI);
                    SKData data = subImage.Encode();

                    //  SKData data = image.Encode();

                    FileStream fs = File.Create(bitmapPath);
                    data.SaveTo(fs);
                    fs.Close();


                    // var listfiles =Directory.GetFiles( dataSvs.ProjectPath.FullName + "/" + CollectionId + "/bitmaps/" );

                }

            }
            else
            {
                // Generate a new file to avoid dublicates = (FileName withoutExtension - GUId.extension)

                var fileName = $"bitmap-{Guid.NewGuid().ToString()}.bmp";
                var path = dataSvs.ProjectPath.FullName + "/" + CollectionId + "/bitmaps/" + fileName;

                if (Directory.Exists(dataSvs.ProjectPath.FullName + "/" + CollectionId + "/bitmaps/"))
                {
                    bitmapPath = dataSvs.ProjectPath.FullName + "/" + CollectionId + "/bitmaps/" + fileName;

                }
                else
                {
                    Directory.CreateDirectory(dataSvs.ProjectPath.FullName + "/" + CollectionId + "/bitmaps/");
                    bitmapPath = dataSvs.ProjectPath.FullName + "/" + CollectionId + "/bitmaps/" + fileName;

                }

                //  bitmapPath = $"bitmap-{Guid.NewGuid().ToString()}.bmp";

            }


            using (SKImage image = SKImage.FromBitmap(dataSvs.saveBitmap))
            {

                SKRectI sKRectI = new SKRectI(0, 0, dataSvs.Wdimension.ThumbWidth, dataSvs.Wdimension.ThumbHeight);
                SKImage subImage = image.Subset(sKRectI);
                SKData data = subImage.Encode();

                //  SKData data = image.Encode();
                FileStream fs = File.Create(bitmapPath);
                data.SaveTo(fs);
                fs.Close();

                dataSvs.editNote.MainImg = bitmapPath;
                // var listfiles =Directory.GetFiles( dataSvs.ProjectPath.FullName + "/" + CollectionId + "/bitmaps/" );

            }

        }

        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round,
            FilterQuality = SKFilterQuality.High,
            BlendMode = SKBlendMode.SrcOver,

            //   Color = SKColors.Red,
            //   StrokeWidth = 8

            PathEffect = SKPathEffect.CreateCorner(45)

        };

        public async void UpdateBitmap()
        {

            using (SKCanvas saveBitmapCanvas = new SKCanvas(dataSvs.saveBitmap))
            {

                //saveBitmapCanvas.Clear();


                foreach (FingerPaintPolyline FngrPaintPoly in dataSvs.completedPolylines)
                {

                    paint.Color = FngrPaintPoly.StrokeColor;
                    paint.StrokeWidth = FngrPaintPoly.StrokeWidth;
                    paint.BlendMode = FngrPaintPoly.StrokeBlendMode;


                    saveBitmapCanvas.DrawPath(FngrPaintPoly.Path, paint);
                }
                /*
                foreach (FingerPaintPolyline FngrPaintPoly in dataSvs.inProgressPolylines.Values)
                {
                saveBitmapCanvas.DrawPath(FngrPaintPoly.Path, paint);
                 }

                 */


                dataSvs.noteEdited = true;


            }




            dataSvs.skiaView.Invalidate();
            dataSvs.NotifyStateChanged();

        }


    }


   

}
