using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using local_image_sender.Desktop.Annotations;
using local_image_sender.Desktop.Classes;
using local_image_sender.TcpServer;

namespace local_image_sender.Desktop
{
    public class MainWindowViewModel: INotifyPropertyChanged
    {
        private string _path;
        private bool _connectedDevice;
        private string _status;
        private Server _server;

        // 

        public MainWindowViewModel()
        {
            Path = Properties.Settings.Default.Path;

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            OpenFolderCommand = new RelayCommand(() =>
            {
                if (!string.IsNullOrEmpty(Path))
                    Process.Start(Path);
            });

            new Thread(SetUpServer) {IsBackground = true}.Start();
        }

        /// <summary>
        ///     Set up the server.
        /// </summary>
        private void SetUpServer()
        {
            _server = new Server();

            _server.ClientConnected += (sender, c) =>
            {
                Status = "Client connected.";
                ConnectedDevice = true;
            };

            _server.ClientDisconnected += (sender, c) =>
            {
                Status = "Client disconnected.";
                ConnectedDevice = false;
            };

            _server.FileTransferring += (sender, c) => { Status = "File transfering ..."; };

            _server.FileTransferCompleted += (sender, completed) => { Status = completed ? "Transfer completed." : "Invalid path, transfer aborted."; };

            _server.FileDestination = Path;

            _server.Start();
        }

        /// <summary>
        ///     On closing the main window, run this function first.
        /// </summary>
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            _server.Stop();
        }

        // 

        /// <summary>
        ///     The last command being ran to the client.
        /// </summary>
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Path to the folder that the files should be transferred to.
        /// </summary>
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged();
                Properties.Settings.Default.Path = Path;
                Properties.Settings.Default.Save();
                if (_server != null)
                    _server.FileDestination = Path;
            }
        }

        /// <summary>
        ///     A device is connected to the server.
        /// </summary>
        public bool ConnectedDevice
        {
            get => _connectedDevice;
            set
            {
                _connectedDevice = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Open up the {Path} folder.
        /// </summary>
        public ICommand OpenFolderCommand { get; set; }

        // 

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}