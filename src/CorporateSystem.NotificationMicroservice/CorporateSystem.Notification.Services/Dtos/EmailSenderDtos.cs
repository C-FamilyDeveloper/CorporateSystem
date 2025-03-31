namespace CorporateSystem.Services.Dtos;

public record struct EmailSendDto(string Token, string Title, string Message, string[] ReceiverEmails);