using System.Collections.Generic;
using System.Threading.Tasks;
using NetworkValidation.Models;

namespace NetworkValidation.Services
{
    public interface ITracertService
    {
        Task<IEnumerable<TracertResultModel>> TraceRouteAsync(string host, int maxHops = 30, int timeout = 5000);
    }
} 