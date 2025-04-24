using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetworkValidation.Models;

namespace NetworkValidation.Services
{
    public interface ITracetrService
    {
        Task<IEnumerable<TracetrResultModel>> TraceRouteAsync(string host, int maxHops = 30, int timeout = 5000);
        Task<IEnumerable<TracetrResultModel>> TraceRouteAsync(string host, int maxHops, int timeout, IProgress<TracetrResultModel> progress);
    }
} 