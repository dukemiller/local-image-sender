using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Android.App;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Widget;
using local_image_sender.Android.Activities;
using local_image_sender.Android.Enums;

namespace local_image_sender.Android.Classes
{
    /// <summary>
    ///     Handles all outward network calls and connections related to sending an image.
    /// </summary>
    public class NetworkHandler
    {
        private string _address;
        private TcpClient _socket;
        private readonly Activity _activity;
        private readonly Action _onConnectedChange;
        private readonly Handler _pollHandler;

        // 

        public NetworkHandler(Activity activity, Action onConnectedChange)
        {
            _activity = activity;
            _onConnectedChange = onConnectedChange;
            _pollHandler = new Handler();
            _pollHandler.PostDelayed(PollForConnection, 1_000);
        }

        // 

        /// <summary>
        ///     Server connection status.
        /// </summary>
        public bool Connected { get; set; }

        // 

        /// <summary>
        ///     Send the image to the connected server.
        /// </summary>
        public bool SendImage(string filename, Drawable image)
        {
            if (!Connected)
            {
                Toast.MakeText(_activity, "There was a connection error", ToastLength.Short)
                     .Show();
                return false;
            }

            try
            {
                var writer = new BinaryWriter(_socket.GetStream());

                writer.Write(1);
                writer.Write(filename);

                var fileData = image.ToByteArray();

                writer.Write(fileData.Length);
                writer.Write(fileData);

                return true;
            }

            catch
            {
                Disconnect();
                return false;
            }
        }

        // 

        /// <summary>
        ///     Maintain a tcpclient connection if the addr and port are correct
        /// </summary>
        private bool IsReachable(string address, int port)
        {
            
            try
            {
                _socket = new TcpClient();
                _socket.Connect(address, port);
                return true;
            }

            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Attempt a connection
        /// </summary>
        private void FindAddressAndConnect()
        {
            _address = Enumerable.Range(0, 256)
                .Select(number => $"192.168.0.{number}")
                .FirstOrDefault(address => IsReachable(address, 8008));
            Connected = _address != null;
            _activity.RunOnUiThread(() => _onConnectedChange());
        }

        /// <summary>
        ///     Close all connection sources and update ui
        /// </summary>
        private void Disconnect()
        {
            Connected = false;
            _socket.Client?.Close();
            _socket?.Close();
            _address = null;
            _activity.RunOnUiThread(() => _onConnectedChange());
        }

        /// <summary>
        ///     Repeatedly poll for collection
        /// </summary>
        private void PollForConnection()
        {
            // If the app is still active, then continue to make connection attempts
            if (MainActivity.ActivityStatus == ActivityStatus.Running)
            {
                // no address found last resolve
                if (!Connected || string.IsNullOrEmpty(_address))
                    FindAddressAndConnect();

                else
                {
                    var stillConnected = !(_socket.Client.Poll(10 * 1000, SelectMode.SelectRead) && _socket.Available == 0);
                    if (!stillConnected)
                        Disconnect();
                }
            }

            // If it's not active, then remove all information if still connected
            else
            {
                if (Connected)
                    Disconnect();
            }

            _pollHandler.PostDelayed(PollForConnection, 10_000);
        }
    }
}