using System;
using System.Net.NetworkInformation;

namespace NetworkValidation.Models
{
    public class NetworkStatusModel
    {
        public DateTime Timestamp { get; }
        public string InterfaceName { get; }
        public string InterfaceDescription { get; }
        public string IPAddress { get; }
        public string SubnetMask { get; }
        public string Gateway { get; }
        public string DNS { get; }
        public string MACAddress { get; }
        public OperationalStatus Status { get; }
        public long Speed { get; }
        public bool IsConnected { get; }
        public string Message { get; }

        public NetworkStatusModel(
            string interfaceName,
            string interfaceDescription,
            string ipAddress,
            string subnetMask,
            string gateway,
            string dns,
            string macAddress,
            OperationalStatus status,
            long speed,
            bool isConnected,
            string message)
        {
            Timestamp = DateTime.Now;
            InterfaceName = interfaceName;
            InterfaceDescription = interfaceDescription;
            IPAddress = ipAddress;
            SubnetMask = subnetMask;
            Gateway = gateway;
            DNS = dns;
            MACAddress = macAddress;
            Status = status;
            Speed = speed;
            IsConnected = isConnected;
            Message = message;
        }
    }
} 