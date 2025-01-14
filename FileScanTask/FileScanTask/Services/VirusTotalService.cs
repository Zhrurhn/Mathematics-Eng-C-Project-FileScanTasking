using System.Net.Http;

namespace FileScanTask
{
    public class VirusTotalService : IDisposable
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public VirusTotalService(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("API Key cannot be null or empty.", nameof(apiKey));
            }

            _apiKey = apiKey;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.virustotal.com/api/v3/")
            };
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<string> GetFileReportAsync(string fileHash)
        {
            if (string.IsNullOrEmpty(fileHash))
            {
                throw new ArgumentException("File hash is required.", nameof(fileHash));
            }

            string endpoint = $"files/{fileHash}";
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-apikey", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // "Dosya bulunamadı" durumunu başarılı sayıyoruz
                        return $"{{\"message\": \"File not found for hash {fileHash}\"}}";
                    }
                    else
                    {
                        string errorDetails = await response.Content.ReadAsStringAsync();
                        throw new Exception($"API Error: {response.StatusCode}, Details: {errorDetails}");
                    }
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Network error while accessing VirusTotal API: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
