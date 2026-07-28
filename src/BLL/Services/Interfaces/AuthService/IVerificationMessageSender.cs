namespace BLL.Services.Interfaces.AuthService;

public interface IEmailVerificationSender
{
    Task SendAsync(string email, string recipientName, string code);
}

public interface ISmsVerificationSender
{
    Task SendAsync(string phoneNumber, string code);
}
