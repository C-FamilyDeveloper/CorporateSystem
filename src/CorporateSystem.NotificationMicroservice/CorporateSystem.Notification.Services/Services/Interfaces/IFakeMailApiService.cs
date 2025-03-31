using CorporateSystem.Services.Dtos;

namespace CorporateSystem.Services.Services.Interfaces;

internal interface IFakeMailApiService
{
    Task SendEmailMessageAsync(SendEmailMessageRequest request, CancellationToken cancellationToken = default);

    Task<GetEmailByTokenResponse> GetEmailByTokenAsync(
        GetEmailByTokenRequest request,
        CancellationToken cancellationToken = default);
}