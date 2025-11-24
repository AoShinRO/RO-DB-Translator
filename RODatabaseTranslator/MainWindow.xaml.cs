using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace RODatabaseTranslator
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            StartButton.Visibility = Visibility.Hidden;
            ProgressText.Visibility = Visibility.Visible;
            string inputPath = InputFilePath.Text;


            if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
            {
                StatusText.Text = "Please select a valid input file.";
                return;
            }

            bool mob_db = Path.GetFileName(inputPath).StartsWith("mob_db", StringComparison.OrdinalIgnoreCase);

            string outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
            Directory.CreateDirectory(outputDir);

            string outputPath = Path.Combine(outputDir, Path.GetFileName(inputPath));

            try
            {
                ProgressBar.Value = 0;
                string[] lines = File.ReadAllLines(inputPath);
                var bodySection = lines.SkipWhile(line => !line.Trim().Equals("Body:"))
                                       .Skip(1)
                                       .ToArray();

                int totalItems = bodySection.Count(line => Regex.IsMatch(line, @"^\s*-\s*Id:"));

                if (totalItems == 0)
                {
                    StatusText.Text = "No items found in 'Body' section.";
                    return;
                }

                string headerSection = string.Join(Environment.NewLine, lines.TakeWhile(line => !line.Trim().Equals("Body:")));
                var translatedItems = new System.Collections.Generic.List<string>();
                int lineIndex = 0;

                while (lineIndex < bodySection.Length)
                {
                    string line = bodySection[lineIndex];

                    // Detecta começo de um item
                    var match = Regex.Match(line, @"^\s*-\s*Id:\s*(\d+)");
                    if (match.Success)
                    {
                        string id = match.Groups[1].Value;

                        // Coleta todas as linhas do item
                        var itemLines = new System.Collections.Generic.List<string>();
                        itemLines.Add(line);
                        lineIndex++;

                        while (lineIndex < bodySection.Length && !Regex.IsMatch(bodySection[lineIndex], @"^\s*-\s*Id:"))
                        {
                            itemLines.Add(bodySection[lineIndex]);
                            lineIndex++;
                        }

                        string original_name = string.Empty;
                        // Substitui apenas o Name
                        for (int i = 0; i < itemLines.Count; i++)
                        {
                            if (itemLines[i].TrimStart().StartsWith("Name:"))
                            {
                                original_name = itemLines[i].Trim()
                                .Substring("Name:".Length)
                                .Trim();
                            }
                        }

                        ProgressText.Text = $"Translating {original_name}";
                        ProgressBar.Value = (double)translatedItems.Count / totalItems * 100;
                        await Task.Delay(10);

                        // Obtém a tradução
                        string translation = await FetchTranslationAsync(id, original_name, mob_db);

                        // Substitui apenas o Name
                        for (int i = 0; i < itemLines.Count; i++)
                        {
                            if (itemLines[i].TrimStart().StartsWith("Name:"))
                            {
                                itemLines[i] = $"    Name: {translation}";
                            }
                        }

                        translatedItems.Add(string.Join("\n", itemLines));
                    }
                    else
                    {
                        lineIndex++;
                    }

                }

                // Monta o arquivo final
                string outputContent = $"{headerSection}\nBody:\n{string.Join("\n", translatedItems)}";
                File.WriteAllText(outputPath, outputContent);
                StartButton.IsEnabled = true;
                StartButton.Visibility = Visibility.Visible;
                ProgressText.Visibility = Visibility.Hidden;
                ProgressBar.Value = 100;

            }
            catch (Exception ex)
            {
                StatusText.Text = $"An error occurred: {ex.Message}";
                StartButton.IsEnabled = true;
                StartButton.Visibility = Visibility.Visible;
                ProgressText.Visibility = Visibility.Hidden;
                ProgressBar.Value = 100;
            }
        }

        private void SelectInputFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "YML files (*.yml)|*.yml"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                InputFilePath.Text = openFileDialog.FileName;
            }
        }
        public static bool IsANSIRanged(string text)
        {
            foreach (char c in text)
            {
                if (c > 255) // U+00FF — qualquer coisa acima é CJK, emojis, etc
                    return false;
            }
            return true;
        }
        private async Task<string> FetchTranslationAsync(string parseID, string original_name, bool mob_db)
        {
            if (Properties.Settings.Default.YOUR_API_KEY == string.Empty || Properties.Settings.Default.YOUR_API_KEY == "Enter your divine-pride APIKEY")
            {
                throw new Exception("Missing DivinePride APIKEY");
            }
            try
            {
                var client = new HttpClient();
                if (LangType.SelectedItem != null)
                    client.DefaultRequestHeaders.Add("Accept-Language", LangType.SelectionBoxItem.ToString());
                string url = string.Empty;

                if (!mob_db)
                    url += $"https://www.divine-pride.net/api/database/Item/";
                else
                    url += $"https://www.divine-pride.net/api/database/Monster/";

                url += $"{parseID}?apiKey={Properties.Settings.Default.YOUR_API_KEY}";

                var response = await client.GetStringAsync(url);
                string pattern = string.Empty;

                if (mob_db)
                    pattern = @"\{[^}]*""name"":\s*""([^""]+)""";
                else
                {
                    pattern = @"""aegisName""\s*:\s*""[^""]+""[\s\S]*?""name""\s*:\s*""([^""]+)""";
                }

                var match = Regex.Match(response, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string name = match.Groups[1].Value;
                    if(IsANSIRanged(name))
                        return RemoveBracketedNumbers(name);
                }
                original_name += " #FailedTranslation";
                return original_name;
            }
            catch
            {
                original_name += " #FailedTranslation";
                return original_name;
            }
        }

        private string RemoveBracketedNumbers(string text)
        {
            return Regex.Replace(text, @"\s*\[\d{1,3}\]", string.Empty);
        }

        private void DIVINEAPIKEY_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Properties.Settings.Default.YOUR_API_KEY == string.Empty || Properties.Settings.Default.YOUR_API_KEY == "Enter your divine-pride APIKEY")
                Properties.Settings.Default.YOUR_API_KEY = string.Empty;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
