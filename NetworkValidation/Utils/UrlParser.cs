using System;
using System.Text.RegularExpressions;

namespace NetworkValidation.Utils
{
    public static class UrlParser
    {
        private static readonly Regex UrlRegex = new Regex(
            @"^(?:(?<scheme>https?):\/\/)?(?<host>[^:\/\s]+)(?::(?<port>\d+))?$",
            RegexOptions.IgnoreCase);

        public static (string host, int? port) ParseUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return (null, null);

            var match = UrlRegex.Match(url);
            if (!match.Success)
                return (null, null);

            string host = match.Groups["host"].Value;
            int? port = null;

            if (match.Groups["port"].Success)
            {
                if (int.TryParse(match.Groups["port"].Value, out int parsedPort))
                {
                    port = parsedPort;
                }
            }
            else
            {
                // 기본 포트 설정
                string scheme = match.Groups["scheme"].Value.ToLower();
                port = scheme == "https" ? 443 : 80;
            }

            return (host, port);
        }
    }
} 