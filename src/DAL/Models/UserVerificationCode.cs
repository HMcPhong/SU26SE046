using DAL.Models.Commons;

namespace DAL.Models;

public class UserVerificationCode : BaseEntity
{
    public Guid UserId { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public int FailedAttempts { get; set; }
    public virtual User User { get; set; } = null!;
}
