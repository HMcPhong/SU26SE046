namespace BLL.DTOs;

public record GenerateShiftsDto(Guid WarehouseId, DateTime Date);
public record CreateReceivingTeamDto(Guid ShiftId, string TeamName, List<Guid> StaffIds);
public record PlanReceivingShiftDto(Guid ShiftId, Guid TeamId);
public record AssignDonationRequestDto(Guid RequestId, Guid TeamId);
public record ConfirmPickupDto(decimal ActualWeight, string? Notes, List<string>? ImageUrls);
public record ReschedulePickupDto(DateTime PickupDate, string? Reason);
public record RejectPickupDto(string Reason);

public class ReceivingBatchDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public Guid ShiftId { get; set; }
    public string ShiftStatus { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string WarehouseAddress { get; set; } = string.Empty;
    public List<ReceivingTeamMemberDto> TeamMembers { get; set; } = [];
    public List<ReceivingRequestDto> Requests { get; set; } = [];
}

public class ReceivingRequestDto
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DonorName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal EstimateWeight { get; set; }
    public decimal? ActualWeight { get; set; }
    public DateTime? PickupDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string DeliveryMethod { get; set; } = string.Empty;
    public List<string>? ImageUrls { get; set; }
}

public record ReceivingTeamMemberDto(Guid Id, string FullName, string PhoneNumber);
public record DispatchRequestDto(Guid Id, string Code, string ContactName, string PhoneNumber,
    string DeliveryMethod, string Address, DateTime? ScheduledDate, Guid WarehouseId, string WarehouseName);
public record DispatchTeamDto(Guid Id, string TeamName, Guid ShiftId, string ShiftName,
    DateTime ShiftDate, string ShiftTime, Guid WarehouseId, List<ReceivingTeamMemberDto> Members);
public record ReceivingDispatchBoardDto(List<DispatchRequestDto> Requests, List<DispatchTeamDto> Teams);
