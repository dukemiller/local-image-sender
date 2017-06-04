namespace local_image_sender.TcpServer.Classes
{
    public class RemoteClient
    {
        public string IpAddress { get; set; }
        public bool Connected { get; set; }
        public override string ToString() => $"RemoteClient {{ IpAddress=\"{IpAddress}\", Connected={Connected} }}";
    }
}