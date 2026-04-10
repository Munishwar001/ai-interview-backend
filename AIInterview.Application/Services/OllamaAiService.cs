using AIInterview.Application.Interface;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace AIInterview.Application.Services
{
    public class OllamaAiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _model;

        public OllamaAiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _baseUrl    = config["Ollama:BaseUrl"] ?? "http://localhost:11434";
            _model      = config["Ollama:Model"]   ?? "llama3.2";
        }

        private async Task<string> CallAsync(string systemPrompt, string userPrompt, double temperature = 0.7)
        {
            var request = new
            {
                model   = _model,
                prompt  = $"{systemPrompt}\n\n{userPrompt}",
                stream  = false,
                options = new { temperature }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ollama failed: {(int)response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("response").GetString() ?? string.Empty;
        }

        public Task<string> AnalyzeResume(string resumeText)
            => CallAsync(
                "You are an expert resume reviewer. Always respond with valid JSON only.",
                $@"Analyze this resume and return JSON with keys: overallScore, summary, strengths, weaknesses, suggestions, missingKeywords, formattingFeedback, atsCompatibility. Return ONLY JSON, no markdown.

Resume:
{resumeText}",
                0.3);

        public Task<string> GenerateJobDescription(string prompt)
            => CallAsync(
                "You are a professional job description writer. Write in plain text, no JSON.",
                prompt,
                0.7);

        public async Task<string> ConductInterview(
            List<string> skills,
            List<(string role, string content)> history,
            string? userMessage)
        {
            var skillList = string.Join(", ", skills);
            var sb = new StringBuilder();

            foreach (var (role, content) in history)
                sb.AppendLine($"{(role == "ai" ? "Assistant" : "User")}: {content}");

            if (!string.IsNullOrWhiteSpace(userMessage))
                sb.AppendLine($"User: {userMessage}");
            else if (history.Count == 0)
                sb.AppendLine("User: Please start the interview by asking me the first question.");

            return await CallAsync(
                $"You are an expert technical interviewer. Candidate skills: {skillList}. Ask one question at a time. After 5 questions end with [INTERVIEW_COMPLETE].",
                sb.ToString(),
                0.7);
        }
    }
}
