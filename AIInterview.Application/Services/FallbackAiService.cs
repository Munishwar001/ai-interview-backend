using AIInterview.Application.Interface;
using Microsoft.Extensions.Logging;

namespace AIInterview.Application.Services
{
    /// <summary>
    /// Tries the primary IAiService (Groq) first.
    /// On any exception falls back to Ollama at http://localhost:11434.
    /// </summary>
    public class FallbackAiService : IAiService
    {
        private readonly IAiService _primary;
        private readonly OllamaAiService _fallback;
        private readonly ILogger<FallbackAiService> _logger;

        public FallbackAiService(
            IAiService primary,
            OllamaAiService fallback,
            ILogger<FallbackAiService> logger)
        {
            _primary  = primary;
            _fallback = fallback;
            _logger   = logger;
        }

        public Task<string> AnalyzeResume(string resumeText)
            => Run(() => _primary.AnalyzeResume(resumeText),
                   () => _fallback.AnalyzeResume(resumeText),
                   nameof(AnalyzeResume));

        public Task<string> GenerateJobDescription(string prompt)
            => Run(() => _primary.GenerateJobDescription(prompt),
                   () => _fallback.GenerateJobDescription(prompt),
                   nameof(GenerateJobDescription));

        public Task<string> ConductInterview(List<string> skills, List<(string role, string content)> history, string? userMessage)
            => Run(() => _primary.ConductInterview(skills, history, userMessage),
                   () => _fallback.ConductInterview(skills, history, userMessage),
                   nameof(ConductInterview));

        private async Task<string> Run(Func<Task<string>> primary, Func<Task<string>> fallback, string op)
        {
            try
            {
                return await primary();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Primary AI ({Op}) failed: {Err}. Falling back to Ollama.", op, ex.Message);
                return await fallback();
            }
        }
    }
}
