using System.Security.Cryptography;
using System.Text;
using ContaCorrente.Domain.Services;

namespace ContaCorrente.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    public string HashPassword(string password, out string salt)
    {
        
        salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var combinedPassword = password + salt;
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedPassword));

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        var combinedPassword = password + salt;
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedPassword));
        var computedHash = Convert.ToBase64String(hashBytes);

        return computedHash == hash;
    }
}