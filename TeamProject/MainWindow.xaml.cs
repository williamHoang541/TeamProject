using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace TeamProject
{
    public partial class MainWindow : Window
    {
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
                string translatedText = await TranslateTextAsync(textToTranslate);
                OutputTextBox.Text = translatedText;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<string> TranslateTextAsync(string text)
        {
            string sourceLanguage = "en";  
            string targetLanguage = "vi";  

            string validSourceLanguage = sourceLanguage.ToLower();
            string validTargetLanguage = targetLanguage.ToLower();

            string requestUrl = $"{apiUrl}?q={Uri.EscapeDataString(text)}&langpair={validSourceLanguage}|{validTargetLanguage}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
                    return result.GetProperty("responseData").GetProperty("translatedText").GetString() ?? string.Empty;
                }
                else
                {
                    throw new Exception($"API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while translating: " + ex.Message, ex);
            }
        }

        private async void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select a file to translate (Support: *.txt)",
                Filter = "Text Files (*.txt)|*.txt"
            };

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                string filePath = dlg.FileName;

                try
                {
                    string fileContent = File.ReadAllText(filePath, Encoding.UTF8);

                    if (string.IsNullOrWhiteSpace(fileContent))
                    {
                        MessageBox.Show("The selected file is empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string translatedContent = await TranslateTextAsync(fileContent);

                    OutputTextBox.Text = translatedContent;

                    string translatedFilePath = Path.Combine(
                        Path.GetDirectoryName(filePath),
                        $"{Path.GetFileNameWithoutExtension(filePath)}_translated.txt"
                    );

                    File.WriteAllText(translatedFilePath, translatedContent, Encoding.UTF8);
                    MessageBox.Show($"Translation saved to: {translatedFilePath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}