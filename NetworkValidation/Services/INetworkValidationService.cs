using System;
using System.Threading.Tasks;

namespace NetworkValidation.Services
{
    public interface INetworkValidationService
    {
        Task<ValidationResult> ValidateConnectionAsync(string ipAddress, int port);
    }

    public class ValidationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public Exception Error { get; set; }
        public TimeSpan ResponseTime { get; set; }

        public static ValidationResult Success(string message, TimeSpan responseTime)
        {
            return new ValidationResult
            {
                IsSuccess = true,
                Message = message,
                ResponseTime = responseTime
            };
        }

        public static ValidationResult Failure(string message, Exception error = null)
        {
            return new ValidationResult
            {
                IsSuccess = false,
                Message = message,
                Error = error
            };
        }
    }
} 