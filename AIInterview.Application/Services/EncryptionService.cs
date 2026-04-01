using AIInterview.Application.Interface;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;

namespace AIInterview.Application.Services
{
    internal class EncryptionService(IDataProtectionProvider provider) : IEncryptionService
    {
        private readonly IDataProtector _protector = provider.CreateProtector("CallNet.Application.Services.EncryptionService.v1");

        public string Encrypt(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                return _protector.Protect(data);
            }
            else
            {
                return "";
            }
        }

        public string Decrypt(string data)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(data))
                {
                    return _protector.Unprotect(data);
                }
                else
                {
                    return "";
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public string GenerateRandomToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return WebEncoders.Base64UrlEncode(randomNumber);
        }

    }
}
