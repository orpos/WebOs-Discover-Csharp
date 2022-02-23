using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Timers;
using System.Collections.Generic;
using Rssdp;

namespace webos_discovery
{
    public class Webos
    {

        private HashSet<string> ips = new HashSet<string>();
        public HashSet<string> find_devices()
        {
            using (var deviceLocator = new SsdpDeviceLocator())
            {
                deviceLocator.NotificationFilter = "urn:schemas-upnp-org:device:MediaRenderer:1";
                var foundDevices = deviceLocator.SearchAsync();
                while (!foundDevices.IsCompleted)
                {
                    System.Threading.Thread.Sleep(100);
                }
                foreach (var foundDevice in foundDevices.Result)
                {
                    var FullDevice = foundDevice.GetDeviceInfo();
                    while (!FullDevice.IsCompleted)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    var full_device = FullDevice.Result;
                    if (full_device.ManufacturerUrl.ToString().ToLower().Contains("lg"))
                    {
                        ips.Add(foundDevice.DescriptionLocation.Host.ToString());
                    }
                }
            }
            return ips;
        }
    }
}
