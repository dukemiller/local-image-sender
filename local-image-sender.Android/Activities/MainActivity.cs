using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Interop;
using local_image_sender.Android.Classes;
using local_image_sender.Android.Enums;
using local_image_sender.Android.Listeners;
using local_image_sender.Android.Models;
using Math = System.Math;
using Uri = Android.Net.Uri;

namespace local_image_sender.Android.Activities
{
    [Activity(Label = "Local network image sender", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private static readonly List<string> Extensions = new List<string> {"jpg", "jpeg", "png"};

        private List<AndroidFile> _files = new List<AndroidFile>();

        private AndroidFile _currentFile;

        private NetworkHandler _network;

        private int _currentIndex;

        private bool _sending;

        public static ActivityStatus ActivityStatus = ActivityStatus.Running;

        public static readonly int RequestCodeOpenDirectory = 1;

        public const int RequestPermissionsReadDirectory = 8;

        // Android

        protected override void OnPause()
        {
            base.OnPause();
            ActivityStatus = ActivityStatus.Stopped;
        }

        protected override void OnStop()
        {
            base.OnStop();
            ActivityStatus = ActivityStatus.Stopped;
        }

        protected override void OnResume()
        {
            base.OnResume();
            ActivityStatus = ActivityStatus.Running;
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            ActivityStatus = ActivityStatus.Running;
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            _network = new NetworkHandler(this, UpdateSendVisibility);
            Image.SetOnTouchListener(new GestureListener(this)
            {
                OnSwipeRight = () => MoveImage(ImageMove.Previous),
                OnSwipeLeft = () => MoveImage(ImageMove.Next)
            });
            UpdateSendVisibility();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestPermissionsReadDirectory:
                {
                    if (grantResults.FirstOrDefault() == Permission.Granted)
                    {
                        var intent = new Intent(Intent.ActionOpenDocumentTree);
                        intent.PutExtra("android.content.extra.SHOW_ADVANCED", true);
                        intent.PutExtra("android.content.extra.FANCY", true);
                        intent.PutExtra("android.content.extra.SHOW_FILESIZE", true);
                        StartActivityForResult(intent, RequestCodeOpenDirectory);
                    }
                    else
                    {
                        Toast.MakeText(this,
                                "Permission is needed to access all folders on the filesystem.",
                                ToastLength.Short)
                            .Show();
                    }
                    return;
                }
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == RequestCodeOpenDirectory && resultCode == Result.Ok)
            {
                bool successful;
                (successful, _files) = RequestFiles(Uri.Parse(Uri.Decode(data.DataString)));

                if (_files.Any())
                {
                    _currentFile = _files.First();
                    _currentIndex = 0;
                    Image.SetImageURI(_currentFile.Uri);
                    ImageText.Text = $"{_currentFile.Name} ({_files.IndexOf(_currentFile) + 1}/{_files.Count})";
                }

                else
                {
                    _currentFile = null;
                    _currentIndex = 0;
                    Image.SetImageURI(null);
                    ImageText.Text = successful
                        ? "Empty directory, load another."
                        : "An error occured retrieving files from this directory.";
                }

                UpdateSendVisibility();
            }
        }

        [Export("loadClick")]
        public void OnLoadOnClick(View view)
        {
            // RequestPermissions(new[] {Manifest.Permission.ManageDocuments}, RequestPermissionsReadDirectory);
            var intent = new Intent(Intent.ActionOpenDocumentTree);
            intent.PutExtra("android.content.extra.SHOW_ADVANCED", true);
            intent.PutExtra("android.content.extra.FANCY", true);
            intent.PutExtra("android.content.extra.SHOW_FILESIZE", true);
            StartActivityForResult(intent, RequestCodeOpenDirectory);
        }

        [Export("sendClick")]
        public async void OnSendOnClick(View view)
        {
            _sending = true;

            // The button should not be enabled if not connected, but as a precaution
            if (_network.Connected)
            {
                try
                {
                    var sent = await Task.Run(() =>_network.SendImage(_currentFile.Name, Image.Drawable));
                    if (sent)
                        Toast.MakeText(this, $"Sent {_currentFile.Name}.", ToastLength.Short).Show();
                }

                catch (Java.Lang.Exception e)
                {
                    Log.Debug("??", e, e.Message);
                    // Error.Visibility = ViewStates.Visible;
                    // Error.Text = e.Message;
                }
            }

            _sending = false;
        }

        // Properties

        public Button Send => FindViewById<Button>(Resource.Id.sendButton);

        public Button Load => FindViewById<Button>(Resource.Id.loadButton);

        public ImageView Image => FindViewById<ImageView>(Resource.Id.imageView);

        public TextView ImageText => FindViewById<TextView>(Resource.Id.imageText);

        public TextView Error => FindViewById<TextView>(Resource.Id.errorText);

        // Methods

        private (bool successful, List<AndroidFile> files) RequestFiles(Uri directoryUri)
        {
            bool successful;
            var collection = new List<AndroidFile>();

            try
            {
                var childrenUri = DocumentsContract.BuildChildDocumentsUriUsingTree(
                    directoryUri, DocumentsContract.GetTreeDocumentId(directoryUri));

                var cursor = ContentResolver.Query(
                    childrenUri, new[]
                    {
                        DocumentsContract.Document.ColumnDisplayName,
                        DocumentsContract.Document.ColumnDocumentId
                    }, null, null, null);

                try
                {
                    while (cursor.MoveToNext())
                    {
                        (var name, var uri) =
                            (cursor.GetString(0), DocumentsContract.BuildDocumentUriUsingTree(directoryUri,
                                cursor.GetString(1)));
                        if (Extensions.Any(ext => name.EndsWith(ext, StringComparison.CurrentCultureIgnoreCase)))
                            collection.Add(new AndroidFile {Name = name, Uri = uri});
                    }

                    successful = true;
                }


                finally
                {
                    cursor.Close();
                }
            }

            catch (Java.Lang.Exception e)
            {
                Log.Debug("??", e, e.Message);
                successful = false;
            }

            return (successful, collection);
        }

        private void UpdateSendVisibility()
        {
            if (_network.Connected && _files.Count > 0)
            {
                Send.Alpha = 1f;
                Send.Clickable = true;
            }
            else
            {
                Send.Alpha = 0.5f;
                Send.Clickable = false;
            }
        }

        private void MoveImage(ImageMove direction)
        {
            if (_files.Count > 0 && !_sending)
            {
                switch (direction)
                {
                    case ImageMove.Next:
                        _currentIndex = Math.Min(_currentIndex + 1, _files.Count - 1);
                        break;
                    case ImageMove.Previous:
                        _currentIndex = Math.Max(_currentIndex - 1, 0);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }
                _currentFile = _files[_currentIndex];
                Image.SetImageURI(_currentFile.Uri);
                ImageText.Text = $"{_currentFile.Name} ({_currentIndex + 1}/{_files.Count})";
            }
        }
    }
}