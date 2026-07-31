namespace BLL.DTOs;

public record RegisterResponse(Guid UserId, string Message);

public class VerifyRegistrationRequest
{
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
}

public class ResendVerificationRequest
{
    public Guid UserId { get; set; }
}

public record VerificationResponse(
    bool EmailConfirmed,
    bool AccountActivated,
    string Message);
