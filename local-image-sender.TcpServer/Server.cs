using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using local_image_sender.TcpServer.Classes;
using local_image_sender.TcpServer.Enums;

namespace local_image_sender.TcpServer
{
    public class Server
    {
        private string _filename = "";

        private int _totalSize;

        private bool _stopRequested;

        private State _state = State.Type;

        private BinaryReader _stream;

        private TcpClient _remoteClient;

        private TcpListener _listener;
        
        public event EventHandler<string> FileTransferring;

        public event EventHandler<bool> FileTransferCompleted;

        public event EventHandler<RemoteClient> ClientConnected;

        public event EventHandler<RemoteClient> ClientDisconnected;

        public event EventHandler<string> MessageReceived;
        
        // 

        public string FileDestination { get; set; } = @"C:\";

        // 

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, 8008);
            _listener.Start(1);

            while (!_stopRequested)
            {
                _remoteClient = _listener.AcceptTcpClient();
                ClientConnected?.Invoke(this, new RemoteClient { IpAddress = _remoteClient.IpAddress(), Connected = true });

                while (!_stopRequested)
                {
                    if (_remoteClient.Available > 0)
                        HandleRequest();

                    if (!Connected())
                    {
                        ClientDisconnected?.Invoke(this, new RemoteClient { IpAddress = _remoteClient.IpAddress(), Connected = true });
                        ResetState();
                        break;
                    }
                }
            }
        }

        public void Stop()
        {
            _stopRequested = true;
            _remoteClient?.Client?.Disconnect(false);
            _remoteClient?.Close();
            _listener.Stop();
            _listener = null;
            ResetState();
        }

        // 

        private void ResetState()
        {
            _filename = "";
            _state = State.Type;
            _totalSize = 0;
            _stream = null;
            _remoteClient = null;
        }

        private void HandleRequest()
        {
            if (_stream == null)
                _stream = new BinaryReader(_remoteClient.GetStream());

            switch (_state)
            {
                case State.Type:
                    var type = (MessageType)_stream.ReadInt32();

                    switch (type)
                    {
                        case MessageType.Message:
                            _state = State.Message;
                            break;
                        case MessageType.File:
                            _state = State.Filename;
                            break;
                        default:
                            break;
                    }

                    break;

                case State.Filename:
                    _filename = _stream.ReadString();
                    _state++;
                    break;

                case State.Size:
                    _totalSize = _stream.ReadInt32();
                    _state++;
                    FileTransferring?.Invoke(this, $"Filename: {_filename}\nSize: {_totalSize}");
                    break;

                case State.ByteData:
                    if (Directory.Exists(FileDestination))
                    {
                        File.WriteAllBytes(Path.Combine(FileDestination, _filename), _stream.ReadBytes(_totalSize));
                        FileTransferCompleted?.Invoke(this, true);
                    }
                    else
                        FileTransferCompleted?.Invoke(this, false);
                    _state = State.Type;
                    break;

                case State.Message:
                    var message = _stream.ReadString();
                    MessageReceived?.Invoke(this, message);
                    _state = State.Type;
                    break;

                default:
                    break;
            }
        }

        private bool Connected()
        {
            return !(_remoteClient.Client.Poll(10 * 1000, SelectMode.SelectRead) && _remoteClient.Available == 0);
        }
    }
}