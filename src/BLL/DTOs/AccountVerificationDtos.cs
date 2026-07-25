namespace BLL.DTOs;

public record RegisterResponse(Guid UserId, string Message);

public class VerifyRegistrationRequest
{
    public Guid UserId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class ResendVerificationRequest
{
    public Guid UserId { get; set; }
    public string Channel { get; set; } = string.Empty;
}

public record VerificationResponse(
    bool EmailConfirmed,
    bool PhoneNumberConfirmed,
    bool AccountActivated,
    string Message);
