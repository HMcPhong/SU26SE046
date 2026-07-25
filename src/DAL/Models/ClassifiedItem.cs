using DAL.Models.Commons;

namespace DAL.Models;

public class ClassifiedItem : BaseEntity
{
    public Guid BatchId { get; set; }
    public Guid? ClassifiedBatchId { get; set; }
    public Guid? ProfileId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string FabricType { get; set; } = string.Empty;
    public string GarmentGroup { get; set; } = string.Empty;
    public string ClothingType { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string TargetUser { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public int ConditionRating { get; set; }
    public string ProcessingDirection { get; set; } = string.Empty;
    public string Status { get; set; } = "Classified";
    public List<string>? ImageUrls { get; set; }
    public string? Notes { get; set; }
    public Guid ClassifiedByStaffId { get; set; }
    public DateTime ClassifiedAt { get; set; }
    public virtual IntakeBatch Batch { get; set; } = null!;
    public virtual ClassifiedBatch? ClassifiedBatch { get; set; }
    public virtual Profile? Profile { get; set; }
    public virtual User ClassifiedByStaff { get; set; } = null!;
    public virtual ICollection<InspectionAnswer> InspectionAnswers { get; set; } = new List<InspectionAnswer>();
}
