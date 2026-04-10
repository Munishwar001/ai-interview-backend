using AIInterview.Application.Interface;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace AIInterview.Application.Services
{
    public class GroqAiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrl = "https://api.groq.com/openai/v1/chat/completions";
        private const string Model   = "llama-3.1-8b-instant"; // fast + free tier

        public GroqAiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Groq:ApiKey"]
                ?? throw new InvalidOperationException("Groq API key missing.");
        }

        private async Task<string> CallAsync(IEnumerable<object> messages, double temperature = 0.3)
        {
            var request = new
            {
                model       = Model,
                messages,
                temperature,
                max_tokens  = 1024
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
            req.Headers.Add("Authorization", $"Bearer {_apiKey}");
            req.Content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(req);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Groq API failed: {(int)response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }

        public async Task<string> AnalyzeResume(string resumeText)
        {
            var messages = new object[]
            {
                new { role = "system", content = "You are an expert resume reviewer. Always respond with valid JSON only." },
                new { role = "user",   content = $@"Analyze the following resume and return a structured JSON response with these exact keys:
{{
  ""overallScore"": <number 0-100>,
  ""summary"": ""<brief overall assessment>"",
  ""strengths"": [""<strength1>"", ...],
  ""weaknesses"": [""<weakness1>"", ...],
  ""suggestions"": [""<suggestion1>"", ...],
  ""missingKeywords"": [""<keyword1>"", ...],
  ""formattingFeedback"": ""<feedback>"",
  ""atsCompatibility"": {{ ""score"": <number 0-100>, ""issues"": [""<issue1>"", ...] }}
}}

Resume:
{resumeText}

Return ONLY the JSON object, no markdown, no extra text." }
            };

            return await CallAsync(messages, 0.3);
        }

        public async Task<string> GenerateJobDescription(string prompt)
        {
            var messages = new object[]
            {
                new { role = "system", content = "You are a professional job description writer. Write clear, concise job descriptions in plain text. Do not return JSON." },
                new { role = "user",   content = prompt }
            };

            return await CallAsync(messages, 0.7);
        }

        public async Task<string> ConductInterview(
            List<string> skills,
            List<(string role, string content)> history,
            string? userMessage)
        {
            var skillList = string.Join(", ", skills);

            var messages = new List<object>
            {
                new
                {
                    role    = "system",
                    content = $@"You are an expert technical interviewer conducting a mock interview.
The candidate's skills are: {skillList}.
Ask one focused technical question at a time based on these skills.
After the candidate answers, give brief constructive feedback (1-2 sentences), then ask the next question.
After 5 questions, provide a short overall performance summary and end with the exact phrase: [INTERVIEW_COMPLETE].
Keep responses concise and professional."
                }
            };

            foreach (var (role, content) in history)
                messages.Add(new { role = role == "ai" ? "assistant" : "user", content });

            if (!string.IsNullOrWhiteSpace(userMessage))
                messages.Add(new { role = "user", content = userMessage });
            else if (history.Count == 0)
                messages.Add(new { role = "user", content = "Please start the interview by asking me the first question." });

            return await CallAsync(messages, 0.7);
        }
    }
}
