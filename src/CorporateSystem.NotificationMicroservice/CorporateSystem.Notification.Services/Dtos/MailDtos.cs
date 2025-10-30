namespace CorporateSystem.Services.Dtos;

public record struct SendMailDto(string Title, string Message, string[] ReceiverEmails);