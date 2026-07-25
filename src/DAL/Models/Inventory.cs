using DAL.Models.Commons;
using DAL.Models.Enum;

namespace DAL.Models
{
    public class Inventory : BaseEntity
    {
        public Guid WarehouseId { get; set; }
        public Guid? AreaGroupId { get; set; }
        public Guid? StorageLocationId { get; set; }
        public Guid? ClassifiedBatchId { get; set; }
        public Guid? ProfileId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string FabricType { get; set; } = string.Empty;
        public string GarmentGroup { get; set; } = string.Empty;
        public string ClothingType { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string TargetUser { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string ProcessingDirection { get; set; } = string.Empty;
        public int ConditionRating { get; set; }
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal ReservedWeight { get; set; }
        public string Status { get; set; } = "Available";
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual AreaGroup? AreaGroup { get; set; }
        public virtual StorageLocation? StorageLocation { get; set; }
        public virtual ClassifiedBatch? ClassifiedBatch { get; set; }
        public virtual Profile? Profile { get; set; }
        public virtual ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
    }
}
