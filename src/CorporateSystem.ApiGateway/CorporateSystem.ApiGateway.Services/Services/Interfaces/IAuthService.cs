using CorporateSystem.ApiGateway.Services.Dtos;

namespace CorporateSystem.ApiGateway.Services.Services.Interfaces;

public interface IAuthService
{
    Task<UserInfo> GetUserInfoAsyncByToken(string token, CancellationToken cancellationToken = default);
}