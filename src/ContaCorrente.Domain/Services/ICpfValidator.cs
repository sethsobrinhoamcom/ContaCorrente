namespace ContaCorrente.Domain.Services;

public interface ICpfValidator
{
    bool IsValid(string cpf);
    string RemoveMask(string cpf);
}