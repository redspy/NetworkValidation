using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace NetworkValidation.Models
{
    public class NetworkStatusModel
    {
        public DateTime Timestamp { get; set; }
        public string InterfaceName { get; set; }
        public NetworkInterfaceType InterfaceType { get; set; }
        public string IPAddress { get; set; }
        public string SubnetMask { get; set; }
        public string Gateway { get; set; }
        public string DNS { get; set; }
        public string MACAddress { get; set; }
        public long Speed { get; set; }
        public OperationalStatus Status { get; set; }
        public string DetailedStatus { get; set; }
        public bool HasIPConflict { get; set; }
        public bool HasInternetAccess { get; set; }
        public string ErrorMessage { get; set; }

        public NetworkStatusModel()
        {
            Timestamp = DateTime.Now;
            InterfaceName = string.Empty;
            IPAddress = string.Empty;
            SubnetMask = string.Empty;
            Gateway = string.Empty;
            DNS = string.Empty;
            MACAddress = string.Empty;
            DetailedStatus = string.Empty;
            ErrorMessage = string.Empty;
        }
    }
} 