namespace CorporateSystem.Services.Dtos;

public record struct SendMailDto(string Token, string Title, string Message, string[] ReceiverEmails);