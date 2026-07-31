using DAL.Models.Commons;

namespace DAL.Models;

/// <summary>
/// Batch-level provenance. Clothing is physically mixed inside an intake batch,
/// so this relation records possible donation sources without making a false
/// item-level attribution.
/// </summary>
public class ClassifiedBatchDonationRequest : BaseEntity
{
    public Guid ClassifiedBatchId { get; set; }
    public Guid DonationRequestId { get; set; }
    public Guid IntakeBatchId { get; set; }
    public DateTime LinkedAt { get; set; }

    public virtual ClassifiedBatch ClassifiedBatch { get; set; } = null!;
    public virtual DonationRequest DonationRequest { get; set; } = null!;
    public virtual IntakeBatch IntakeBatch { get; set; } = null!;
}
