using AIInterview.Application.Interface;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace AIInterview.Application.Services
{
    public class GeminiAiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string Model = "gemini-2.0-flash";  

        public GeminiAiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"]
                ?? throw new InvalidOperationException("Gemini API key missing.");
        }

        public async Task<string> AnalyzeResume(string resumeText)
        {
            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={_apiKey}";

                var prompt = $@"
You are an expert resume reviewer and career coach. Analyze the following resume and return a structured JSON response with these exact keys:
{{
  ""overallScore"": <number 0-100>,
  ""summary"": ""<brief overall assessment>"",
  ""strengths"": [""<strength1>"", ""<strength2>"", ...],
  ""weaknesses"": [""<weakness1>"", ""<weakness2>"", ...],
  ""suggestions"": [""<actionable suggestion1>"", ""<actionable suggestion2>"", ...],
  ""missingKeywords"": [""<keyword1>"", ""<keyword2>"", ...],
  ""formattingFeedback"": ""<feedback on resume structure and formatting>"",
  ""atsCompatibility"": {{
    ""score"": <number 0-100>,
    ""issues"": [""<issue1>"", ""<issue2>"", ...]
  }}
}}

Resume:
{resumeText}

Return ONLY the JSON object, no markdown, no extra text.";

                var requestBody = new
                {
                    system_instruction = new
                    {
                        parts = new[] { new { text = "You are an expert resume reviewer. Always respond with valid JSON only." } }
                    },
                    contents = new[]
                    {
                        new { role = "user", parts = new[] { new { text = prompt } } }
                    },
                    generationConfig = new { temperature = 0.3 }
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Gemini API failed: {(int)response.StatusCode} - {error}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);

                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                    throw new Exception("Gemini returned an empty response.");

                return text;
            }
            catch (Exception) { throw; }
        }

        public async Task<string> ConductInterview(List<string> skills, List<(string role, string content)> history, string? userMessage)
        {
            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={_apiKey}";

                var skillList = string.Join(", ", skills);
                var systemPrompt = $@"You are an expert technical interviewer conducting a mock interview.
The candidate's skills are: {skillList}.
Ask one focused technical question at a time based on these skills.
After the candidate answers, give brief constructive feedback (1-2 sentences), then ask the next question.
After 5 questions, provide a short overall performance summary and end with the exact phrase: [INTERVIEW_COMPLETE].
Keep responses concise and professional.";

                // Build conversation history
                var contents = new List<object>();

                foreach (var (role, content) in history)
                {
                    contents.Add(new
                    {
                        role = role == "ai" ? "model" : "user",
                        parts = new[] { new { text = content } }
                    });
                }

                // Add current user message if provided
                if (!string.IsNullOrWhiteSpace(userMessage))
                {
                    contents.Add(new
                    {
                        role = "user",
                        parts = new[] { new { text = userMessage } }
                    });
                }

                // Gemini requires at least one content entry
                if (contents.Count == 0)
                {
                    contents.Add(new
                    {
                        role = "user",
                        parts = new[] { new { text = "Please start the interview by asking me the first question." } }
                    });
                }

                var requestBody = new
                {
                    system_instruction = new
                    {
                        parts = new[] { new { text = systemPrompt } }
                    },
                    contents,
                    generationConfig = new { temperature = 0.7 }
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Gemini API failed: {(int)response.StatusCode} - {error}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);

                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                    throw new Exception("Gemini returned an empty response.");

                return text;
            }
            catch (Exception) { throw; }
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

                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                    throw new Exception("Gemini returned an empty response.");

                return text;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}