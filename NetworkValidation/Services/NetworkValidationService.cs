using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkValidation.Services
{
    public class NetworkValidationService : INetworkValidationService
    {
        public async Task<ValidationResult> ValidateConnectionAsync(string ipAddress, int port)
        {
            try
            {
                var startTime = DateTime.Now;
                
                using (var client = new TcpClient())
                {
                    var connectTask = client.ConnectAsync(ipAddress, port);
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
                    
                    var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                    
                    if (completedTask == timeoutTask)
                    {
                        return ValidationResult.Failure("Connection timed out");
                    }
                    
                    var responseTime = DateTime.Now - startTime;
                    return ValidationResult.Success($"Successfully connected to {ipAddress}:{port}", responseTime);
                }
            }
            catch (SocketException ex)
            {
                return ValidationResult.Failure($"Connection failed: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure($"An error occurred: {ex.Message}", ex);
            }
        }
    }
} 