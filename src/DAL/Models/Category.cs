using DAL.Models.Commons;

namespace DAL.Models
{
    public class Category : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public int SortOrder { get; set; }
        public string Description { get; set; } = string.Empty;
        public int? MinimumMatchCount { get; set; }
    }
}
