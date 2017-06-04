using System.IO;
using Android.Graphics;
using Android.Graphics.Drawables;

namespace local_image_sender.Android.Classes
{
    public static class Extensions
    {
        public static byte[] ToByteArray(this Drawable d)
        {
            var bitmap = ((BitmapDrawable) d).Bitmap;
            using (var stream = new MemoryStream())
            {
                bitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
                return stream.ToArray();
            }
        }


    }
}