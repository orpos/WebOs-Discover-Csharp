using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Timers;
using System.Collections.Generic;

namespace webos_discovery
{
    public class UPnPDiscovery
    {
        private const string searchRequest = @"M-SEARCH * HTTP/1.1
HOST: {0}:{1}
MAN: ""ssdp:discover""
MX: {2}
ST: {3}

";
        private const string MulticastIP = "239.255.255.250";

        public bool ended = false;
        private HashSet<string> requests_responses = new HashSet<string>();

        private const int multicastPort = 1900;

        private const int multicastTTL = 4;

        private const int MaxResultSize = 8096;

        private const string DefaultDeviceType = "urn:schemas-upnp-org:device:MediaRenderer:1";

        private int searchTimeOut = 5; // Seconds

        private Socket socket;

        private SocketAsyncEventArgs sendEvent;

        public void StartFindDevices() {
            string request = string.Format(searchRequest, MulticastIP, multicastPort, this.searchTimeOut, DefaultDeviceType);
            byte[] multiCastData = Encoding.UTF8.GetBytes(request);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SendBufferSize = multiCastData.Length;
            sendEvent = new SocketAsyncEventArgs();
            sendEvent.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(MulticastIP), multicastPort);
            sendEvent.SetBuffer(multiCastData, 0, multiCastData.Length);
            sendEvent.Completed += OnSocketSendEventCompleted;

            Timer t = new Timer(TimeSpan.FromSeconds(this.searchTimeOut + 1).TotalMilliseconds);
            t.Elapsed += (e, s) => {
                if (socket == null)
                {
                    t.Stop();
                    return;
                }
                socket.Dispose();
                socket = null;
            };
            // Kick off the initial Send
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, IPAddress.Parse(MulticastIP).GetAddressBytes());
            socket.SendToAsync(sendEvent);
            t.Start();
        }

        private void OnSocketSendEventCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                ended = true;
                return;
            }

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.SendTo:
                    e.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receiveBuffer = new byte[MaxResultSize];
                    socket.ReceiveBufferSize = receiveBuffer.Length;
                    e.SetBuffer(receiveBuffer, 0, MaxResultSize);
                    socket.ReceiveFromAsync(e);
                    break;

                case SocketAsyncOperation.ReceiveFrom:
                    string result = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);
                    if (result.StartsWith("HTTP/1.1 200 OK", StringComparison.InvariantCultureIgnoreCase))
                    {
                        requests_responses.Add(result);
                    }
                    else
                    {
                        Console.WriteLine("INVALID SEARCH RESPONSE\n" + result);
                    }
                    if (socket != null)// and kick off another read
                        socket.ReceiveFromAsync(e);
                    break;
                default:
                    break;
            }
        }
        public HashSet<string> get_requests_responses()
        {
            return requests_responses;
        }
    }
    public class others
    {
        public string read_location(string resp, string keyword)
        {
            foreach (string line_ in resp.Split(Environment.NewLine.ToCharArray()))
            {
                string line = line_.ToLower();
                string header = "location: ";
                if (line.StartsWith(header))
                {
                    return line.Substring(header.Length);
                }
            }
            return "";
        }
        public string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
        public bool validate_location(string location, string keyword, int timeout = 5)
        {
            string content = Get(location);
            if (keyword.Length == 0)
            {
                return true;
            }
            return (content.Contains(keyword));
        }
    }
    public class Client
    {
        public HashSet<string> get_ips()
        {
            UPnPDiscovery Connx = new UPnPDiscovery();
            Connx.StartFindDevices();
            HashSet<string> seen = new HashSet<string>();
            HashSet<string> locations = new HashSet<string>();
            while (!Connx.ended)
            {
                System.Threading.Thread.Sleep(100);
            }
            foreach (string response in Connx.get_requests_responses())
            {
                string location = new others().read_location(response, "lg");
                if (location != "" && !seen.Contains(location))
                {
                    seen.Add(location);
                    if (new others().validate_location(location, "lg"))
                    {
                        locations.Add(location);
                    }
                }
            }
            HashSet<string> hostnames = new HashSet<string>();
            foreach (string x in locations)
            {
                hostnames.Add(new Uri(x).Host);
            }
            return hostnames;
        }
    }
}
