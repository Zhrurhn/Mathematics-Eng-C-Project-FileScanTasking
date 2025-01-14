using Microsoft.Win32;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Windows;


namespace FileScanTask
{
    public partial class MainWindow : Window
    {
        private readonly TaskManager _taskManager;
        private readonly VirusTotalService _virusTotalService;

        private readonly string _jsonDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Results", "Log_Json");
        private readonly string _csvDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Results", "Csv");

        public MainWindow()
        {
            InitializeComponent();
            _taskManager = new TaskManager();

            try
            {
                string apiKey = LoadApiKey();
                _virusTotalService = new VirusTotalService(apiKey);

                EnsureDirectoriesExist();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Failed to initialize VirusTotalService", ex);
            }
        }

        private void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(_jsonDirectory);
            Directory.CreateDirectory(_csvDirectory);
        }

        private string LoadApiKey()
        {
            string configFile = "appsettings.json";

            if (!File.Exists(configFile))
            {
                throw new FileNotFoundException("Configuration file not found.", configFile);
            }

            try
            {
                string json = File.ReadAllText(configFile);
                var document = System.Text.Json.JsonDocument.Parse(json);

                if (document.RootElement.TryGetProperty("VirusTotalApiKey", out var apiKeyElement))
                {
                    string apiKey = apiKeyElement.GetString();

                    if (string.IsNullOrWhiteSpace(apiKey))
                    {
                        throw new Exception("API key is empty in configuration file.");
                    }

                    return apiKey;
                }
                throw new Exception("API key property is missing in configuration file.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while reading API key: {ex.Message}");
            }
        }

        private void AddFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var filePath in openFileDialog.FileNames)
                {
                    try
                    {
                        _taskManager.AddToQueue(filePath);
                        UpdateFileListUI();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to add file '{Path.GetFileName(filePath)}': {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void StartScan_Click(object sender, RoutedEventArgs e)
        {
            if (_taskManager.IsQueueEmpty())
            {
                MessageBox.Show("No files to scan.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.Value = 0;
                ProgressBar.Maximum = _taskManager.GetAllFiles().Count();

                var filesToProcess = _taskManager.GetAllFiles().ToList();

                foreach (var file in filesToProcess)
                {
                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(TimeSpan.FromSeconds(60));

                    try
                    {
                        ProgressBar.Value++;

                        if (string.IsNullOrEmpty(file.Hash))
                        {
                            MessageBox.Show($"The file '{file.FileName}' does not have a valid hash. Skipping...", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }

                        string jsonResponse = await _virusTotalService.GetFileReportAsync(file.Hash);

                        // Save the scan report files
                        SaveReportFiles(file, jsonResponse);

                        // Generate the email body
                        string emailBody = GenerateEmailBody(
                            file.FileName,
                            file.Hash,
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            jsonResponse
                        );

                        // Send the email report
                        SendEmailReport("example@hotmail.com", "Scan Report", emailBody);

                        // Remove the processed file from the queue
                        _taskManager.RemoveFromQueue(file);
                        UpdateFileListUI();
                    }
                    catch (OperationCanceledException)
                    {
                        MessageBox.Show($"Timeout while scanning {file.FileName}. Moving to the next file.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage("Error processing file", ex);
                    }
                }

                MessageBox.Show("All scans completed.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error during scanning", ex);
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Hidden;
            }
        }

        private string GenerateEmailBody(string fileName, string fileHash, string scanDate, string jsonResponse)
        {
            var bodyBuilder = new System.Text.StringBuilder();

            // Add introduction and report details
            bodyBuilder.AppendLine("If the answer is the right solution, please click Accept Answer and kindly upvote it. If you have extra questions about this answer, please click Comment.");
            bodyBuilder.AppendLine();
            bodyBuilder.AppendLine("Scan Report:");
            bodyBuilder.AppendLine("------------");
            bodyBuilder.AppendLine($"File Name: {fileName}");
            bodyBuilder.AppendLine($"Hash (SHA256): {fileHash.Substring(0, 8)}***{fileHash.Substring(fileHash.Length - 4)}");
            bodyBuilder.AppendLine($"Scan Date: {scanDate}");

            // Add other details (e.g., analysis stats, size)
            // Parse JSON here...

            bodyBuilder.AppendLine("------------");
            return bodyBuilder.ToString();
        }



        private void SaveReportFiles(FileScan file, string jsonResponse)
        {
            try
            {
                string jsonFilePath = Path.Combine(_jsonDirectory, $"{file.FileName}_report.json");
                string csvFilePath = Path.Combine(_csvDirectory, $"{file.FileName}_report.csv");

                File.WriteAllText(jsonFilePath, jsonResponse);
                File.WriteAllText(csvFilePath, ConvertJsonToCsv(jsonResponse));
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error saving report files", ex);
            }
        }

        public void SendEmailReport(string toEmail, string subject, string body)
        {
            try
            {
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("example@gmail.com", "12345"),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("example@gmail.com"),
                    Subject = subject,
                    Body = body
                };

                mailMessage.To.Add(toEmail);

                smtpClient.Send(mailMessage);
                Console.WriteLine("Email sent successfully.");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"SMTP error: {ex.Message}");
                ShowErrorMessage("Error sending email", ex);


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                ShowErrorMessage("Error sending email", ex);
            }
        }




        private string ConvertJsonToCsv(string jsonResponse)
        {
            try
            {
                var csvBuilder = new System.Text.StringBuilder();
                csvBuilder.AppendLine("Key,Value");

                var document = System.Text.Json.JsonDocument.Parse(jsonResponse);
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    csvBuilder.AppendLine($"{property.Name},{property.Value}");
                }

                return csvBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to convert JSON to CSV: {ex.Message}");
            }
        }

        private void UpdateFileListUI()
        {
            FileList.Items.Clear();

            foreach (var file in _taskManager.GetAllFiles())
            {
                FileList.Items.Add(file.GetDisplayText());
            }
        }

        private void ShowErrorMessage(string title, Exception ex)
        {
            string errorMessage = ex != null ? ex.Message : "Unknown error occurred.";
            MessageBox.Show($"{title}: {errorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}
