using System;
using System.Net;

namespace NetworkValidation.Models
{
    public class TracetrResultModel
    {
        public int Hop { get; set; }
        public string HostName { get; set; }
        public IPAddress IPAddress { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public string FormattedResponseTime { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }

        public TracetrResultModel(int hop, string hostName, IPAddress ipAddress, TimeSpan responseTime, bool isSuccess, string errorMessage = null)
        {
            Hop = hop;
            HostName = hostName;
            IPAddress = ipAddress;
            ResponseTime = responseTime;
            FormattedResponseTime = $"{responseTime.TotalMilliseconds:F2}ms";
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }
    }
} 