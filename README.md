# local-image-sender

A very basic dekstop file sender using a desktop tcp server and android client. I use **Xamarin.Android** for the Android implementation and **WPF** for the desktop implementation, basically using the standard libraries. Included also is a TCP server implementation specific to this project I use and reference the desktop.

### Use

**Setting up the server:** Open up the server and put in a valid filepath.  

**Setting up the client:** Select a folder that contains the images to be sent to the desktop. On connection, press send to send the file to the desktop.  

**How it works:** On client open, there will be a pulse to search for available servers of this type on the network. If no connections are found, then the search will retry every 10 seconds. The client is able to select folders without being connected, but is unable to send until a connection is found.

### Bugs, Todo's and etc

\- The client will only look in the network range 192.168.0.xxx for the server. This is a relatively easy thing to fix.  
\- Access of folders on the android device gave varying results, and so some folders require extra permissions that I have to figure out how to request (notably the camera pictures / DCIM folder).   
\- The filebrowser is a little weird, probably wanting to switch to a gallery view soon.  
\- Needs more fluent image switching / transition, very stiff at the moment.  
\- Some problems with sending images (wrong resolution, slow to convert, ...)
