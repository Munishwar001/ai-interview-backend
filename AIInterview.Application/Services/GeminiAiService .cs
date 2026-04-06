using AIInterview.Application.Interface;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AIInterview.Application.Services
{
    public class GeminiAiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string Model = "gemini-2.5-flash-lite";

        public GeminiAiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"]
                ?? throw new InvalidOperationException("Gemini API key missing.");
        }

        public async Task<string> GenerateJobDescription(string prompt)
        {
            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={_apiKey}";

                var requestBody = new
                {
                    system_instruction = new
                    {
                        parts = new[]
                        {
                            new { text = "You are a helpful AI that generates job descriptions." }
                        }
                    },
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7
                    }
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Gemini API failed: {(int)response.StatusCode} - {error}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseJson);

                return doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}