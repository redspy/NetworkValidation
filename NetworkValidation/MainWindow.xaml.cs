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
        private readonly ITracetrService _tracetrService;
        private readonly ObservableCollection<ValidationResultModel> _validationResults;
        private readonly ObservableCollection<TracetrResultModel> _tracetrResults;

        public MainWindow()
        {
            InitializeComponent();
            _validationService = new NetworkValidationService();
            _tracetrService = new TracetrService();
            _validationResults = new ObservableCollection<ValidationResultModel>();
            _tracetrResults = new ObservableCollection<TracetrResultModel>();
            ResultsList.ItemsSource = _validationResults;
            TracetrResultsList.ItemsSource = _tracetrResults;

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

        private async void TracetrButton_Click(object sender, RoutedEventArgs e)
        {
            var host = IpAddressTextBox.Text.Trim();
            if (string.IsNullOrEmpty(host))
            {
                MessageBox.Show("Please enter a host address");
                return;
            }

            if (!int.TryParse(TimeoutTextBox.Text, out int timeout) || timeout <= 0)
            {
                MessageBox.Show("Please enter a valid timeout value (greater than 0)");
                return;
            }

            TracetrButton.IsEnabled = false;
            TracetrButton.Content = "Tracing...";
            _tracetrResults.Clear();

            var progress = new Progress<TracetrResultModel>(result =>
            {
                _tracetrResults.Add(result);
            });

            try
            {
                await _tracetrService.TraceRouteAsync(host, 30, timeout, progress);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                TracetrButton.IsEnabled = true;
                TracetrButton.Content = "Tracetr";
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
