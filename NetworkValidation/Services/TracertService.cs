using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using NetworkValidation.Models;

namespace NetworkValidation.Services
{
    public class TracertService : ITracertService
    {
        public async Task<IEnumerable<TracertResultModel>> TraceRouteAsync(string host, int maxHops = 30, int timeout = 5000)
        {
            var results = new List<TracertResultModel>();
            var ping = new Ping();
            var options = new PingOptions(1, true);
            var buffer = new byte[32];
            var stopwatch = new System.Diagnostics.Stopwatch();

            for (int ttl = 1; ttl <= maxHops; ttl++)
            {
                options.Ttl = ttl;
                stopwatch.Restart();

                try
                {
                    var reply = await ping.SendPingAsync(host, timeout, buffer, options);
                    stopwatch.Stop();

                    if (reply.Status == IPStatus.Success || reply.Status == IPStatus.TtlExpired)
                    {
                        string hostName = string.Empty;
                        try
                        {
                            var hostEntry = await Dns.GetHostEntryAsync(reply.Address);
                            hostName = hostEntry.HostName;
                        }
                        catch
                        {
                            hostName = "Unknown";
                        }

                        results.Add(new TracertResultModel(
                            ttl,
                            hostName,
                            reply.Address,
                            stopwatch.Elapsed,
                            true
                        ));

                        if (reply.Status == IPStatus.Success)
                            break;
                    }
                    else
                    {
                        results.Add(new TracertResultModel(
                            ttl,
                            "Request timed out",
                            null,
                            TimeSpan.Zero,
                            false,
                            $"Status: {reply.Status}"
                        ));
                    }
                }
                catch (Exception ex)
                {
                    results.Add(new TracertResultModel(
                        ttl,
                        "Error",
                        null,
                        TimeSpan.Zero,
                        false,
                        ex.Message
                    ));
                    break;
                }
            }

            return results;
        }
    }
} 