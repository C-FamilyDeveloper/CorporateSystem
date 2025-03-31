using CorporateSystem.Services.Dtos;

namespace CorporateSystem.Services.Extensions;

internal static class ValidationDtoExtensions
{
    public static void MustBeValid(this EmailSendDto dto)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dto.Message);
        ArgumentException.ThrowIfNullOrWhiteSpace(dto.Token);
        ArgumentException.ThrowIfNullOrWhiteSpace(dto.Title);
        ArgumentNullException.ThrowIfNull(dto.ReceiverEmails);
    }

    public static void MustBeValid(this SendMailDto dto)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dto.Message);
        ArgumentException.ThrowIfNullOrWhiteSpace(dto.Token);
        ArgumentException.ThrowIfNullOrWhiteSpace(dto.Title);
        ArgumentNullException.ThrowIfNull(dto.ReceiverEmails);
    }
}