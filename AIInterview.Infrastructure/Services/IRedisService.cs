namespace AIInterview.Infrastructure.Services
{
    public interface IRedisService
    {
        Task<string?> GetAsync(string key);
        Task RemoveAsync(string key);
        Task SetAsync(string key, string value, TimeSpan? expiry = null);
    }
}