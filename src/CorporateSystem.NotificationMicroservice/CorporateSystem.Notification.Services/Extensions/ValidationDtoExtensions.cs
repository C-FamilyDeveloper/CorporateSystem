using CorporateSystem.Services.Dtos;
using Microsoft.Extensions.Logging;

namespace CorporateSystem.Services.Extensions;

internal static class ValidationDtoExtensions
{
    public static void MustBeValid<T>(this EmailSendDto dto, ILogger<T> logger)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.Message);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.Token);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.Title);
            ArgumentNullException.ThrowIfNull(dto.ReceiverEmails);
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(MustBeValid)}: {e.Message}");
        }
    }

    public static void MustBeValid<T>(this SendMailDto dto, ILogger<T> logger)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.Message);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.Token);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.Title);
            ArgumentNullException.ThrowIfNull(dto.ReceiverEmails);
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(MustBeValid)}: {e.Message}");
        }
    }
}