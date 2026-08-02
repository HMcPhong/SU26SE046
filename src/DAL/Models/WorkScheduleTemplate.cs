using DAL.Models.Commons;

namespace DAL.Models;

public class WorkScheduleTemplate : BaseEntity
{
    public Guid WarehouseId { get; set; }
    public int Year { get; set; }
    public string WorkingDays { get; set; } = "1,2,3,4,5";
    public TimeSpan MorningStartTime { get; set; }
    public TimeSpan MorningEndTime { get; set; }
    public TimeSpan AfternoonStartTime { get; set; }
    public TimeSpan AfternoonEndTime { get; set; }
    public virtual Warehouse Warehouse { get; set; } = null!;
}
