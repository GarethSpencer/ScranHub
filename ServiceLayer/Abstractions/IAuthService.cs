using Utilities.Models.Requests.Authentication;

namespace ServiceLayer.Abstractions
{
    public interface IAuthService
    {
        bool ValidateCredentials(AuthenticationDataRequest data);
        string GenerateToken(Guid id, string userName, string firstName, string surname);
    }
}