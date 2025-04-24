using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using NetworkValidation.Models;

namespace NetworkValidation.Services
{
    public class NetworkStatusService : INetworkStatusService
    {
        public async Task<IEnumerable<NetworkStatusModel>> GetNetworkStatusAsync()
        {
            return await Task.Run(() =>
            {
                var results = new List<NetworkStatusModel>();
                try
                {
                    var interfaces = NetworkInterface.GetAllNetworkInterfaces();

                    foreach (var ni in interfaces)
                    {
                        var model = new NetworkStatusModel
                        {
                            InterfaceName = ni.Name,
                            InterfaceType = ni.NetworkInterfaceType,
                            Speed = ni.Speed,
                            Status = ni.OperationalStatus
                        };

                        try
                        {
                            var ipProperties = ni.GetIPProperties();
                            
                            // Get IPv4 address
                            var ipv4 = ipProperties.UnicastAddresses
                                .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                            if (ipv4 != null)
                            {
                                model.IPAddress = ipv4.Address.ToString();
                                model.SubnetMask = ipv4.IPv4Mask.ToString();
                            }

                            // Get Gateway
                            var gateway = ipProperties.GatewayAddresses
                                .FirstOrDefault(g => g.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                            if (gateway != null)
                            {
                                model.Gateway = gateway.Address.ToString();
                            }

                            // Get DNS
                            var dns = ipProperties.DnsAddresses
                                .FirstOrDefault(d => d.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                            if (dns != null)
                            {
                                model.DNS = dns.ToString();
                            }

                            // Get MAC Address
                            model.MACAddress = BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes())
                                .Replace("-", ":");

                            // Check for IP conflict
                            model.HasIPConflict = CheckForIPConflict(ipProperties);

                            // Check internet access
                            model.HasInternetAccess = CheckInternetAccess();

                            // Set detailed status
                            model.DetailedStatus = GetDetailedStatus(model);
                        }
                        catch (Exception ex)
                        {
                            model.ErrorMessage = $"Error getting interface details: {ex.Message}";
                        }

                        results.Add(model);
                    }
                }
                catch (Exception ex)
                {
                    results.Add(new NetworkStatusModel
                    {
                        ErrorMessage = $"Error getting network interfaces: {ex.Message}"
                    });
                }

                return results;
            });
        }

        private bool CheckForIPConflict(IPInterfaceProperties ipProperties)
        {
            try
            {
                // Check for duplicate IP addresses
                var ipv4Addresses = ipProperties.UnicastAddresses
                    .Where(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Select(ip => ip.Address);

                foreach (var address in ipv4Addresses)
                {
                    using (var ping = new Ping())
                    {
                        var reply = ping.Send(address, 1000);
                        if (reply.Status == IPStatus.Success && reply.Address.ToString() != address.ToString())
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // If we can't check, assume no conflict
                return false;
            }

            return false;
        }

        private bool CheckInternetAccess()
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    using (var stream = client.OpenRead("http://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private string GetDetailedStatus(NetworkStatusModel model)
        {
            var statusList = new List<string>();

            // Basic status
            statusList.Add($"Status: {model.Status}");

            // IP conflict check
            if (model.HasIPConflict)
            {
                statusList.Add("IP Conflict Detected");
            }

            // Internet access check
            if (!model.HasInternetAccess)
            {
                statusList.Add("No Internet Access");
            }

            // Interface type specific checks
            switch (model.InterfaceType)
            {
                case NetworkInterfaceType.Ethernet:
                    if (model.Speed == 0)
                        statusList.Add("Ethernet Cable Not Connected");
                    break;
                case NetworkInterfaceType.Wireless80211:
                    if (model.Speed == 0)
                        statusList.Add("WiFi Signal Weak or Not Connected");
                    break;
            }

            // Gateway check
            if (string.IsNullOrEmpty(model.Gateway))
            {
                statusList.Add("No Gateway Configured");
            }

            // DNS check
            if (string.IsNullOrEmpty(model.DNS))
            {
                statusList.Add("No DNS Server Configured");
            }

            return string.Join(", ", statusList);
        }
    }
} 