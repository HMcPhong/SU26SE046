using DAL.Models.Commons;

namespace DAL.Models;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? DonationRequestId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TargetUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public User User { get; set; } = null!;
    public DonationRequest? DonationRequest { get; set; }
}
