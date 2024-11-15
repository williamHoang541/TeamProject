using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace TeamProject
{
    public partial class MainWindow : Window
    {
        // Reuse HttpClient for better performance
        private static readonly HttpClient client = new HttpClient();
        private readonly string apiUrl = "https://api.mymemory.translated.net/get"; // MyMemory API URL

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void TranslateButton_Click(object sender, RoutedEventArgs e)
        {
            string textToTranslate = InputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(textToTranslate))
            {
                MessageBox.Show("Please enter text to translate.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Translate text
                string translatedText = await TranslateTextAsync(textToTranslate);
                OutputTextBox.Text = translatedText;
            }
            catch (Exception ex)
            {
                // Show error message if an exception occurs
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<string> TranslateTextAsync(string text)
        {
            string sourceLanguage = "en";  // Định dạng ngôn ngữ nguồn là tiếng Anh
            string targetLanguage = "vi";  // Định dạng ngôn ngữ đích là tiếng Việt

            // Chuyển đổi ngôn ngữ thành chữ thường
            string validSourceLanguage = sourceLanguage.ToLower();
            string validTargetLanguage = targetLanguage.ToLower();

            // Tạo URL yêu cầu API với mã ngôn ngữ đúng
            string requestUrl = $"{apiUrl}?q={Uri.EscapeDataString(text)}&langpair={validSourceLanguage}|{validTargetLanguage}";

            try
            {
                // Gửi yêu cầu GET tới API
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Nếu thành công, phân tích phản hồi và lấy văn bản đã dịch
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
                    return result.GetProperty("responseData").GetProperty("translatedText").GetString() ?? string.Empty;
                }
                else
                {
                    // Nếu có lỗi API, ném ra ngoại lệ
                    throw new Exception($"API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                // Xử lý các ngoại lệ khác có thể xảy ra
                throw new Exception("An error occurred while translating: " + ex.Message, ex);
            }
        }
    }
}