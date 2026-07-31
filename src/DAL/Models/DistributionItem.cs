using DAL.Models.Commons;

namespace DAL.Models
{
    public class DistributionItem : BaseEntity
    {
        public Guid DistributionRequestId { get; set; }
        public Guid? ProfileId { get; set; }
        public Guid InventoryId { get; set; }
        public int ConditionRating { get; set; }
        public int RequestedQuantity { get; set; }
        public int ApprovedQuantity { get; set; }
        public int IssuedQuantity { get; set; }
        public decimal RequestedWeight { get; set; }
        public decimal IssuedWeight { get; set; }
        public string? Notes { get; set; }
        public virtual DistributionRequest DistributionRequest { get; set; } = null!;
        public virtual Profile? Profile { get; set; }
        public virtual Inventory Inventory { get; set; } = null!;
    }
}
