using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using NetworkValidation.Models;

namespace NetworkValidation.Services
{
    public class TracetrService : ITracetrService
    {
        private string GetDetailedTimeoutMessage(IPStatus status, int ttl, int timeout, string host)
        {
            switch (status)
            {
                case IPStatus.TimedOut:
                    return $"타임아웃 발생 (TTL: {ttl})\n" +
                           $"가능한 원인:\n" +
                           $"1. 라우터/방화벽이 ICMP 패킷을 차단 중\n" +
                           $"2. 네트워크 지연 ({timeout}ms 초과)\n" +
                           $"3. 대상 호스트({host})가 응답하지 않음\n" +
                           $"해결 방법:\n" +
                           $"1. 타임아웃 시간 증가\n" +
                           $"2. 방화벽 설정 확인\n" +
                           $"3. 네트워크 연결 상태 확인";

                case IPStatus.DestinationHostUnreachable:
                    return $"대상 호스트에 도달할 수 없음 (TTL: {ttl})\n" +
                           $"가능한 원인:\n" +
                           $"1. 호스트가 다운됨\n" +
                           $"2. 호스트의 방화벽이 차단\n" +
                           $"3. 잘못된 IP 주소";

                case IPStatus.DestinationNetworkUnreachable:
                    return $"대상 네트워크에 도달할 수 없음 (TTL: {ttl})\n" +
                           $"가능한 원인:\n" +
                           $"1. 라우팅 테이블 오류\n" +
                           $"2. 네트워크 분할\n" +
                           $"3. 잘못된 네트워크 설정";

                case IPStatus.TtlExpired:
                    return $"TTL 만료 (TTL: {ttl})\n" +
                           $"가능한 원인:\n" +
                           $"1. 패킷이 너무 많은 홉을 거침\n" +
                           $"2. 라우팅 루프 발생";

                default:
                    return $"상태: {status} (TTL: {ttl})";
            }
        }

        private TracetrResultModel CreateFinalStatusResult(string host, bool isSuccess, string message)
        {
            return new TracetrResultModel(
                0, // TTL 0은 최종 상태를 나타냄
                isSuccess ? "✅ 트레이스 성공" : "❌ 트레이스 실패",
                null,
                TimeSpan.Zero,
                isSuccess,
                isSuccess ? 
                    $"대상 호스트 {host}에 성공적으로 도달했습니다." :
                    message
            );
        }

        public async Task<IEnumerable<TracetrResultModel>> TraceRouteAsync(string host, int maxHops = 30, int timeout = 5000)
        {
            var results = new List<TracetrResultModel>();
            var ping = new Ping();
            var options = new PingOptions(1, true);
            var buffer = new byte[32];
            var stopwatch = new System.Diagnostics.Stopwatch();
            bool reachedDestination = false;

            for (int ttl = 1; ttl <= maxHops; ttl++)
            {
                options.Ttl = ttl;
                stopwatch.Restart();

                try
                {
                    var reply = await ping.SendPingAsync(host, timeout, buffer, options);
                    stopwatch.Stop();

                    TracetrResultModel result;
                    if (reply.Status == IPStatus.Success || reply.Status == IPStatus.TtlExpired)
                    {
                        string hostName = string.Empty;
                        IPAddress ipAddress = reply.Address;
                        try
                        {
                            var hostEntry = await Dns.GetHostEntryAsync(reply.Address);
                            hostName = hostEntry.HostName;
                        }
                        catch (Exception)
                        {
                            hostName = reply.Address.ToString();
                        }

                        result = new TracetrResultModel(
                            ttl,
                            hostName,
                            ipAddress,
                            stopwatch.Elapsed,
                            true
                        );

                        if (reply.Status == IPStatus.Success)
                        {
                            reachedDestination = true;
                            results.Add(result);
                            break;
                        }
                    }
                    else
                    {
                        string errorMessage = GetDetailedTimeoutMessage(reply.Status, ttl, timeout, host);
                        result = new TracetrResultModel(
                            ttl,
                            "Request timed out",
                            null,
                            TimeSpan.Zero,
                            false,
                            errorMessage
                        );
                    }

                    results.Add(result);
                }
                catch (Exception ex)
                {
                    var result = new TracetrResultModel(
                        ttl,
                        "Error",
                        null,
                        TimeSpan.Zero,
                        false,
                        $"에러 발생 (TTL: {ttl})\n" +
                        $"상세 정보:\n" +
                        $"{ex.Message}\n" +
                        $"해결 방법:\n" +
                        $"1. 네트워크 연결 확인\n" +
                        $"2. 호스트 주소 확인\n" +
                        $"3. 방화벽 설정 확인"
                    );
                    results.Add(result);
                    break;
                }
            }

            // 최종 상태 결과 추가
            if (!reachedDestination)
            {
                results.Add(CreateFinalStatusResult(host, false, 
                    $"대상 호스트 {host}에 도달하지 못했습니다.\n" +
                    $"최대 홉 수({maxHops})에 도달했거나 네트워크 문제가 발생했습니다."));
            }
            else
            {
                results.Add(CreateFinalStatusResult(host, true, null));
            }

            return results;
        }

        public async Task<IEnumerable<TracetrResultModel>> TraceRouteAsync(string host, int maxHops, int timeout, IProgress<TracetrResultModel> progress)
        {
            var results = new List<TracetrResultModel>();
            var ping = new Ping();
            var options = new PingOptions(1, true);
            var buffer = new byte[32];
            var stopwatch = new System.Diagnostics.Stopwatch();
            bool reachedDestination = false;

            for (int ttl = 1; ttl <= maxHops; ttl++)
            {
                options.Ttl = ttl;
                stopwatch.Restart();

                try
                {
                    var reply = await ping.SendPingAsync(host, timeout, buffer, options);
                    stopwatch.Stop();

                    TracetrResultModel result;
                    if (reply.Status == IPStatus.Success || reply.Status == IPStatus.TtlExpired)
                    {
                        string hostName = string.Empty;
                        IPAddress ipAddress = reply.Address;
                        try
                        {
                            var hostEntry = await Dns.GetHostEntryAsync(reply.Address);
                            hostName = hostEntry.HostName;
                        }
                        catch (Exception)
                        {
                            hostName = reply.Address.ToString();
                        }

                        result = new TracetrResultModel(
                            ttl,
                            hostName,
                            ipAddress,
                            stopwatch.Elapsed,
                            true
                        );

                        if (reply.Status == IPStatus.Success)
                        {
                            reachedDestination = true;
                            progress?.Report(result);
                            results.Add(result);
                            break;
                        }
                    }
                    else
                    {
                        string errorMessage = GetDetailedTimeoutMessage(reply.Status, ttl, timeout, host);
                        result = new TracetrResultModel(
                            ttl,
                            "Request timed out",
                            null,
                            TimeSpan.Zero,
                            false,
                            errorMessage
                        );
                    }

                    progress?.Report(result);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    var result = new TracetrResultModel(
                        ttl,
                        "Error",
                        null,
                        TimeSpan.Zero,
                        false,
                        $"에러 발생 (TTL: {ttl})\n" +
                        $"상세 정보:\n" +
                        $"{ex.Message}\n" +
                        $"해결 방법:\n" +
                        $"1. 네트워크 연결 확인\n" +
                        $"2. 호스트 주소 확인\n" +
                        $"3. 방화벽 설정 확인"
                    );
                    progress?.Report(result);
                    results.Add(result);
                    break;
                }
            }

            // 최종 상태 결과 추가
            var finalResult = reachedDestination ? 
                CreateFinalStatusResult(host, true, null) :
                CreateFinalStatusResult(host, false, 
                    $"대상 호스트 {host}에 도달하지 못했습니다.\n" +
                    $"최대 홉 수({maxHops})에 도달했거나 네트워크 문제가 발생했습니다.");
            
            progress?.Report(finalResult);
            results.Add(finalResult);

            return results;
        }
    }
} 