using System;

namespace NetworkValidation.Models
{
    public class ValidationResultModel
    {
        public string Timestamp { get; set; }
        public string Message { get; set; }
        public string ResponseTime { get; set; }
        public bool IsSuccess { get; set; }

        public ValidationResultModel(DateTime timestamp, string message, TimeSpan responseTime, bool isSuccess)
        {
            Timestamp = timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            Message = message;
            ResponseTime = $"Response Time: {responseTime.TotalMilliseconds:F2}ms";
            IsSuccess = isSuccess;
        }
    }
} 