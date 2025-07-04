using CorporateSystem.Services.Dtos;
using Microsoft.Extensions.Logging;

namespace CorporateSystem.Services.Extensions;

internal static class ValidationDtoExtensions
{
    public static void MustBeValid<T>(this EmailSendDto emailSendDto, ILogger<T> logger)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(emailSendDto.Message);
            ArgumentException.ThrowIfNullOrWhiteSpace(emailSendDto.Token);
            ArgumentException.ThrowIfNullOrWhiteSpace(emailSendDto.Title);
            ArgumentNullException.ThrowIfNull(emailSendDto.ReceiverEmails);
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(MustBeValid)}: {e.Message}");
            throw;
        }
    }

    public static void MustBeValid<T>(this SendMailDto sendMailDto, ILogger<T> logger)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sendMailDto.Message);
            ArgumentException.ThrowIfNullOrWhiteSpace(sendMailDto.Token);
            ArgumentException.ThrowIfNullOrWhiteSpace(sendMailDto.Title);
            ArgumentNullException.ThrowIfNull(sendMailDto.ReceiverEmails);
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(MustBeValid)}: {e.Message}");
            throw;
        }
    }
}