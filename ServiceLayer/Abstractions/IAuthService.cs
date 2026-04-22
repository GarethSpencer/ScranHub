using Utilities.Models.Requests;

namespace ServiceLayer.Abstractions
{
    public interface IAuthService
    {
        bool ValidateCredentials(AuthenticationDataRequest data);
        string GenerateToken(Guid id, string userName, string firstName, string surname);
    }
}