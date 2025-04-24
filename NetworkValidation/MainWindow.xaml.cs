using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using NetworkValidation.Models;
using NetworkValidation.Services;

namespace NetworkValidation
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly INetworkValidationService _validationService;
        private readonly ObservableCollection<ValidationResultModel> _validationResults;

        public MainWindow()
        {
            InitializeComponent();
            _validationService = new NetworkValidationService();
            _validationResults = new ObservableCollection<ValidationResultModel>();
            ResultsList.ItemsSource = _validationResults;

#if DEBUG
            // 디버그 모드에서만 기본값 설정
            IpAddressTextBox.Text = "192.168.219.1";
            PortTextBox.Text = "80";
#endif
        }

        private async void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = IpAddressTextBox.Text.Trim();
            string portText = PortTextBox.Text.Trim();

            // IP 주소 유효성 검사
            if (!System.Net.IPAddress.TryParse(ipAddress, out System.Net.IPAddress parsedIp))
            {
                AddResult(false, "Invalid IP Address format", TimeSpan.Zero);
                return;
            }

            // Port 번호 유효성 검사
            if (!int.TryParse(portText, out int port) || port < 1 || port > 65535)
            {
                AddResult(false, "Port number must be between 1 and 65535", TimeSpan.Zero);
                return;
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
