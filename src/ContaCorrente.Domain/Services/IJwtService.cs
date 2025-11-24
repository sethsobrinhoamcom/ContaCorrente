namespace ContaCorrente.Domain.Services;

public interface IJwtService
{
    string GenerateToken(string idContaCorrente, string numeroConta, string cpf);
    (bool IsValid, string? IdContaCorrente) ValidateToken(string token);
}