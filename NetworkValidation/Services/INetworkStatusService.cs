using System.Collections.Generic;
using System.Threading.Tasks;
using NetworkValidation.Models;

namespace NetworkValidation.Services
{
    public interface INetworkStatusService
    {
        Task<IEnumerable<NetworkStatusModel>> GetNetworkStatusAsync();
    }
} 