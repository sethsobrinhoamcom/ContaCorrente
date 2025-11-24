using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ContaCorrente.Domain.Services;

namespace ContaCorrente.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string idContaCorrente, string numeroConta, string cpf)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "sua-chave-super-secreta-de-no-minimo-32-caracteres-para-seguranca");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id_conta_corrente", idContaCorrente),
                new Claim("numero_conta", numeroConta),
                new Claim("cpf", cpf),
                new Claim(ClaimTypes.NameIdentifier, idContaCorrente),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(int.Parse(_configuration["Jwt:ExpirationHours"] ?? "2")),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"] ?? "BankMore",
            Audience = _configuration["Jwt:Audience"] ?? "BankMoreAPI"
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public (bool IsValid, string? IdContaCorrente) ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return (false, null);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "sua-chave-super-secreta-de-no-minimo-32-caracteres-para-seguranca");

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "BankMore",
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "BankMoreAPI",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var idContaCorrente = jwtToken.Claims.First(x => x.Type == "id_conta_corrente").Value;

            return (true, idContaCorrente);
        }
        catch
        {
            return (false, null);
        }
    }
}