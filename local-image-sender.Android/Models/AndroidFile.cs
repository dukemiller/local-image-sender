using Android.Net;

namespace local_image_sender.Android.Models
{
    public class AndroidFile
    {
        /// <summary>
        ///     The android filesystem uri.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        ///     The cosmetic name.
        /// </summary>
        public string Name { get; set; }
    }
}