using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using NetworkValidation.Models;
using NetworkValidation.Services;
using NetworkValidation.Utils;

namespace NetworkValidation
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly INetworkValidationService _validationService;
        private readonly ITracertService _tracertService;
        private readonly ObservableCollection<ValidationResultModel> _validationResults;
        private readonly ObservableCollection<TracertResultModel> _tracertResults;

        public MainWindow()
        {
            InitializeComponent();
            _validationService = new NetworkValidationService();
            _tracertService = new TracertService();
            _validationResults = new ObservableCollection<ValidationResultModel>();
            _tracertResults = new ObservableCollection<TracertResultModel>();
            ResultsList.ItemsSource = _validationResults;
            TracertResultsList.ItemsSource = _tracertResults;

#if DEBUG
            // 디버그 모드에서만 기본값 설정
            IpAddressTextBox.Text = "192.168.219.1";
            PortTextBox.Text = "80";
#endif
        }

        private async void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            string input = IpAddressTextBox.Text.Trim();
            string portText = PortTextBox.Text.Trim();

            string ipAddress;
            int port;

            // URL 파싱 시도
            var (host, parsedPort) = UrlParser.ParseUrl(input);
            if (host != null)
            {
                ipAddress = host;
                port = parsedPort ?? 80; // URL에 포트가 없으면 기본값 80 사용
            }
            else
            {
                // 기존 IP 주소 검증 방식
                if (!System.Net.IPAddress.TryParse(input, out System.Net.IPAddress parsedIp))
                {
                    AddResult(false, "Invalid IP Address or URL format", TimeSpan.Zero);
                    return;
                }
                ipAddress = input;

                // 포트 번호 검증
                if (!int.TryParse(portText, out port) || port < 1 || port > 65535)
                {
                    AddResult(false, "Port number must be between 1 and 65535", TimeSpan.Zero);
                    return;
                }
            }

            // 버튼 비활성화
            ValidateButton.IsEnabled = false;
            ValidateButton.Content = "Validating...";

            try
            {
                var result = await _validationService.ValidateConnectionAsync(ipAddress, port);
                AddResult(result.IsSuccess, result.Message, result.ResponseTime);
            }
            finally
            {
                // 버튼 상태 복원
                ValidateButton.IsEnabled = true;
                ValidateButton.Content = "Validate";
            }
        }

        private async void TracertButton_Click(object sender, RoutedEventArgs e)
        {
            string input = IpAddressTextBox.Text.Trim();

            string host;
            // URL 파싱 시도
            var (parsedHost, _) = UrlParser.ParseUrl(input);
            if (parsedHost != null)
            {
                host = parsedHost;
            }
            else
            {
                // IP 주소 검증
                if (!System.Net.IPAddress.TryParse(input, out System.Net.IPAddress parsedIp))
                {
                    AddResult(false, "Invalid IP Address or URL format", TimeSpan.Zero);
                    return;
                }
                host = input;
            }

            // 버튼 비활성화
            TracertButton.IsEnabled = false;
            TracertButton.Content = "Tracing...";
            _tracertResults.Clear();

            try
            {
                var results = await _tracertService.TraceRouteAsync(host);
                foreach (var result in results)
                {
                    _tracertResults.Add(result);
                }
            }
            finally
            {
                // 버튼 상태 복원
                TracertButton.IsEnabled = true;
                TracertButton.Content = "Tracert";
            }
        }

        private void AddResult(bool isSuccess, string message, TimeSpan responseTime)
        {
            var resultModel = new ValidationResultModel(
                DateTime.Now,
                message,
                responseTime,
                isSuccess
            );

            _validationResults.Insert(0, resultModel);
        }
    }
}
