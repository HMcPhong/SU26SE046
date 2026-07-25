using System.Text.RegularExpressions;
using BLL.DTOs;
using BLL.Services.Interfaces.ReceivingOperations;
using DAL;
using DAL.Models;
using DAL.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Implements.ReceivingOperations;

public class ReceivingOperationsService(AppDbContext context) : IReceivingOperationsService
{
    public async Task GenerateStandardShiftsAsync(GenerateShiftsDto dto)
    {
        var date = dto.Date.Date;
        if (!await context.Warehouses.AnyAsync(x => x.Id == dto.WarehouseId && x.IsActive != false))
            throw new InvalidOperationException("Warehouse not found.");

        var definitions = new[]
        {
            ("Ca sáng", new TimeSpan(8, 0, 0), new TimeSpan(11, 0, 0)),
            ("Ca chiều", new TimeSpan(13, 0, 0), new TimeSpan(17, 0, 0))
        };

        foreach (var definition in definitions)
        {
            var exists = await context.Shifts.AnyAsync(x => x.WarehouseId == dto.WarehouseId
                && x.ShiftDate == date && x.StartTime == definition.Item2 && x.IsActive != false);
            if (exists) continue;
            context.Shifts.Add(new Shift
            {
                Id = Guid.NewGuid(), WarehouseId = dto.WarehouseId, ShiftDate = date,
                ShiftName = definition.Item1, StartTime = definition.Item2, EndTime = definition.Item3,
                Status = "Scheduled", CreateAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();
    }

    public async Task<GenerateYearShiftsResultDto> GenerateYearShiftsAsync(GenerateYearShiftsDto dto)
    {
        if (dto.Year is < 2020 or > 2100)
            throw new InvalidOperationException("Year must be between 2020 and 2100.");
        if (!await context.Warehouses.AnyAsync(x => x.Id == dto.WarehouseId && x.IsActive != false))
            throw new InvalidOperationException("Warehouse not found.");

        // Fixed-date Vietnamese public holidays. Lunar holidays such as Tet and Hung Kings
        // are supplied by Manager for the selected year because their solar dates change.
        var excludedDates = new HashSet<DateTime>
        {
            new(dto.Year, 1, 1),
            new(dto.Year, 4, 30),
            new(dto.Year, 5, 1),
            new(dto.Year, 9, 2)
        };
        foreach (var holiday in dto.HolidayDates ?? [])
        {
            if (holiday.Year != dto.Year)
                throw new InvalidOperationException("Every additional holiday must belong to the selected year.");
            excludedDates.Add(holiday.Date);
        }

        var yearStart = new DateTime(dto.Year, 1, 1);
        var yearEnd = new DateTime(dto.Year + 1, 1, 1);
        var existing = await context.Shifts.AsNoTracking()
            .Where(x => x.WarehouseId == dto.WarehouseId && x.ShiftDate >= yearStart
                && x.ShiftDate < yearEnd && x.IsActive != false)
            .Select(x => new { Date = x.ShiftDate.Date, x.StartTime })
            .ToListAsync();
        var existingKeys = existing.Select(x => (x.Date, x.StartTime)).ToHashSet();
        var definitions = new[]
        {
            ("Ca sáng", new TimeSpan(8, 0, 0), new TimeSpan(11, 0, 0)),
            ("Ca chiều", new TimeSpan(13, 0, 0), new TimeSpan(17, 0, 0))
        };
        var workingDays = 0;
        var created = 0;
        var skipped = 0;
        for (var date = yearStart; date < yearEnd; date = date.AddDays(1))
        {
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
                || excludedDates.Contains(date.Date))
                continue;
            workingDays++;
            foreach (var definition in definitions)
            {
                if (existingKeys.Contains((date.Date, definition.Item2)))
                {
                    skipped++;
                    continue;
                }
                context.Shifts.Add(new Shift
                {
                    Id = Guid.NewGuid(), WarehouseId = dto.WarehouseId, ShiftDate = date.Date,
                    ShiftName = definition.Item1, StartTime = definition.Item2,
                    EndTime = definition.Item3, Status = "Scheduled", CreateAt = DateTime.UtcNow
                });
                created++;
            }
        }
        await context.SaveChangesAsync();
        return new GenerateYearShiftsResultDto(workingDays, created, skipped);
    }

    public async Task<Guid> CreateTeamAsync(CreateReceivingTeamDto dto)
    {
        if (dto.StaffIds.Distinct().Count() != 2)
            throw new InvalidOperationException("A receiving team must have exactly two different staff members.");
        var shift = await context.Shifts.FirstOrDefaultAsync(x => x.Id == dto.ShiftId && x.IsActive != false)
            ?? throw new InvalidOperationException("Shift not found.");
        if (shift.Status != "Scheduled")
            throw new InvalidOperationException("A team can only be created for a scheduled shift.");
        if (await context.OperationalTeams.AnyAsync(x => x.ShiftId == shift.Id && x.IsActive != false))
            throw new InvalidOperationException("This shift already has a receiving team.");
        var validStaff = await context.Users.Include(x => x.Role).CountAsync(x => dto.StaffIds.Contains(x.Id)
            && x.Role.RoleName == "ReceivingStaff" && x.IsActive != false);
        if (validStaff != 2) throw new InvalidOperationException("Both members must be active ReceivingStaff users.");
        var overlappingStaff = await context.TeamMembers
            .Where(x => dto.StaffIds.Contains(x.StaffId) && x.IsActive != false
                && x.Team.IsActive != false && x.Team.Shift.IsActive != false
                && x.Team.Shift.ShiftDate == shift.ShiftDate
                && x.Team.Shift.Status != "Completed"
                && x.Team.Shift.StartTime < shift.EndTime
                && shift.StartTime < x.Team.Shift.EndTime)
            .Select(x => x.Staff.FullName).Distinct().ToListAsync();
        if (overlappingStaff.Count != 0)
            throw new InvalidOperationException(
                $"Staff already assigned to an overlapping shift: {string.Join(", ", overlappingStaff)}.");

        var team = new OperationalTeam
        {
            Id = Guid.NewGuid(), ShiftId = shift.Id, TeamName = dto.TeamName,
            TeamType = "Receiving", CreateAt = DateTime.UtcNow
        };
        context.OperationalTeams.Add(team);
        context.TeamMembers.AddRange(dto.StaffIds.Select(id => new TeamMember
        {
            Id = Guid.NewGuid(), TeamId = team.Id, StaffId = id, CreateAt = DateTime.UtcNow
        }));
        await context.SaveChangesAsync();
        return team.Id;
    }

    public async Task<int> PlanShiftAsync(PlanReceivingShiftDto dto)
    {
        var shift = await context.Shifts.FirstOrDefaultAsync(x => x.Id == dto.ShiftId && x.IsActive != false)
            ?? throw new InvalidOperationException("Shift not found.");
        var team = await context.OperationalTeams.Include(x => x.Members)
            .FirstOrDefaultAsync(x => x.Id == dto.TeamId && x.ShiftId == shift.Id && x.IsActive != false)
            ?? throw new InvalidOperationException("Receiving team does not belong to this shift.");
        if (team.Members.Count(x => x.IsActive != false) != 2)
            throw new InvalidOperationException("Receiving team must contain exactly two members.");

        var alreadyPlanned = context.PickupAssignments.Where(x => x.IsActive != false).Select(x => x.DonorRequestId);
        var candidates = await context.DonationRequests.Include(x => x.Donor)
            .Where(x => x.WarehouseId == shift.WarehouseId && x.IsActive != false
                && x.Status == DonationRequestStatus.WaitingReceivingStaff
                && x.DeliveryMethod == "StaffPickup"
                && x.PickupDate.HasValue && x.PickupDate.Value.Date <= shift.ShiftDate.Date
                && !alreadyPlanned.Contains(x.Id))
            .OrderBy(x => x.PickupDate)
            .ThenBy(x => x.PickupAddress)
            .ToListAsync();

        if (candidates.Count == 0) return 0;

        var batch = await context.IntakeBatches
            .FirstOrDefaultAsync(x => x.ShiftId == shift.Id && x.IsActive != false);
        if (batch is null)
        {
            var areas = candidates.Select(x => ExtractArea(x.PickupAddress)).Distinct().ToList();
            batch = new IntakeBatch
            {
                Id = Guid.NewGuid(), WarehouseId = shift.WarehouseId, ShiftId = shift.Id, ReceivingTeamId = team.Id,
                IntakeDate = shift.ShiftDate.Date.Add(shift.StartTime), BatchCode = $"INT-{shift.ShiftDate:yyyyMMdd}-{Guid.NewGuid():N}"[..22].ToUpperInvariant(),
                RouteName = string.Join(" → ", areas), Status = "Planned", CreateAt = DateTime.UtcNow
            };
            context.IntakeBatches.Add(batch);
        }
        else if (batch.ReceivingTeamId != team.Id)
        {
            throw new InvalidOperationException("This shift already has an intake batch assigned to another team.");
        }

        var planned = 0;
        var order = await context.PickupAssignments
            .Where(x => x.IntakeBatchId == batch.Id && x.IsActive != false)
            .Select(x => (int?)x.RouteOrder).MaxAsync() ?? 0;
        foreach (var request in candidates.OrderBy(x => ExtractArea(x.PickupAddress)).ThenBy(x => x.PickupAddress))
        {
            var area = ExtractArea(request.PickupAddress);
            context.PickupAssignments.Add(new PickupAssignment
            {
                Id = Guid.NewGuid(), DonorRequestId = request.Id, ShiftId = shift.Id, TeamId = team.Id,
                IntakeBatchId = batch.Id, RouteOrder = ++order, AreaKey = area,
                Status = "Pending", CreateAt = DateTime.UtcNow
            });
            request.Status = DonationRequestStatus.ReceivingStaffAssigned;
            request.UpdateAt = DateTime.UtcNow;
            planned++;
        }
        await context.SaveChangesAsync();
        return planned;
    }

    public async Task<ReceivingDispatchBoardDto> GetDispatchBoardAsync()
    {
        var assignedIds = context.PickupAssignments.Where(x => x.IsActive != false)
            .Select(x => x.DonorRequestId);
        var requests = await context.DonationRequests.AsNoTracking()
            .Include(x => x.Warehouse)
            .Where(x => x.IsActive != false
                && (x.Status == DonationRequestStatus.WaitingReceivingStaff
                    || x.Status == DonationRequestStatus.PendingStaffAssign)
                && !assignedIds.Contains(x.Id))
            .OrderBy(x => x.PickupDate).ThenBy(x => x.CreateAt)
            .Select(x => new DispatchRequestDto(
                x.Id, $"DR-{x.CreateAt!.Value.Year}-{x.Id.ToString().Substring(0, 8).ToUpper()}",
                x.ContactName, x.ContactPhoneNumber, x.DeliveryMethod, x.PickupAddress,
                x.PickupDate, x.WarehouseId, x.Warehouse.WarehouseName))
            .ToListAsync();

        var teams = await context.OperationalTeams.AsNoTracking()
            .Include(x => x.Shift)
            .Include(x => x.Members).ThenInclude(x => x.Staff)
            .Where(x => x.IsActive != false && x.TeamType == "Receiving"
                && x.Shift.IsActive != false && x.Shift.Status != "Completed")
            .OrderBy(x => x.Shift.ShiftDate).ThenBy(x => x.Shift.StartTime)
            .Select(x => new DispatchTeamDto(
                x.Id, x.TeamName, x.ShiftId, x.Shift.ShiftName, x.Shift.ShiftDate,
                $"{x.Shift.StartTime:hh\\:mm} - {x.Shift.EndTime:hh\\:mm}", x.Shift.WarehouseId,
                x.Members.Where(m => m.IsActive != false)
                    .Select(m => new ReceivingTeamMemberDto(m.StaffId, m.Staff.FullName, m.Staff.PhoneNumber)).ToList()))
            .ToListAsync();
        return new ReceivingDispatchBoardDto(requests, teams);
    }

    public async Task<ManagerReceivingSetupDto> GetManagerSetupAsync()
    {
        var warehouses = await context.Warehouses.AsNoTracking()
            .Where(x => x.IsActive != false).OrderBy(x => x.WarehouseName)
            .Select(x => new ManagerWarehouseOptionDto(x.Id, x.WarehouseName, x.Address)).ToListAsync();
        var staff = await context.Users.AsNoTracking()
            .Where(x => x.IsActive != false && x.Role.RoleName == "ReceivingStaff")
            .OrderBy(x => x.FullName)
            .Select(x => new ManagerStaffOptionDto(x.Id, x.FullName, x.UserName, x.PhoneNumber)).ToListAsync();
        var shifts = await context.Shifts.AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Teams.Where(t => t.IsActive != false))
                .ThenInclude(x => x.Members.Where(m => m.IsActive != false)).ThenInclude(x => x.Staff)
            .Include(x => x.IntakeBatch)!.ThenInclude(x => x!.PickupAssignments)
            .Where(x => x.IsActive != false)
            .OrderByDescending(x => x.ShiftDate).ThenBy(x => x.StartTime)
            .ToListAsync();
        var shiftDtos = shifts.Select(x =>
        {
            var team = x.Teams.FirstOrDefault();
            return new ManagerShiftOverviewDto(x.Id, x.WarehouseId, x.Warehouse.WarehouseName,
                x.ShiftName, x.ShiftDate, x.StartTime, x.EndTime, x.Status,
                team is null ? null : new ManagerTeamOverviewDto(team.Id, team.TeamName,
                    team.Members.Select(m => new ReceivingTeamMemberDto(
                        m.StaffId, m.Staff.FullName, m.Staff.PhoneNumber)).ToList()),
                x.IntakeBatch?.Id, x.IntakeBatch?.BatchCode, x.IntakeBatch?.Status,
                x.IntakeBatch?.RouteName, x.IntakeBatch?.TotalWeight ?? 0,
                x.IntakeBatch?.PickupAssignments.Count(a => a.IsActive != false) ?? 0);
        }).ToList();
        return new ManagerReceivingSetupDto(warehouses, staff, shiftDtos);
    }

    public async Task AssignRequestAsync(AssignDonationRequestDto dto)
    {
        var request = await context.DonationRequests.Include(x => x.Warehouse)
            .FirstOrDefaultAsync(x => x.Id == dto.RequestId && x.IsActive != false)
            ?? throw new InvalidOperationException("Donation request not found.");
        if (await context.PickupAssignments.AnyAsync(x => x.DonorRequestId == dto.RequestId && x.IsActive != false))
            throw new InvalidOperationException("Donation request is already assigned.");
        var team = await context.OperationalTeams.Include(x => x.Shift).Include(x => x.Members)
            .FirstOrDefaultAsync(x => x.Id == dto.TeamId && x.IsActive != false && x.TeamType == "Receiving")
            ?? throw new InvalidOperationException("Receiving team not found.");
        if (team.Members.Count(x => x.IsActive != false) != 2)
            throw new InvalidOperationException("Receiving team must contain exactly two members.");
        if (team.Shift.WarehouseId != request.WarehouseId)
            throw new InvalidOperationException("The team and donation request must belong to the same warehouse.");

        var batch = await context.IntakeBatches.FirstOrDefaultAsync(x => x.ShiftId == team.ShiftId && x.IsActive != false);
        if (batch is null)
        {
            batch = new IntakeBatch
            {
                Id = Guid.NewGuid(), WarehouseId = request.WarehouseId, ShiftId = team.ShiftId,
                ReceivingTeamId = team.Id, IntakeDate = team.Shift.ShiftDate.Date.Add(team.Shift.StartTime),
                BatchCode = $"INT-{team.Shift.ShiftDate:yyyyMMdd}-{Guid.NewGuid():N}"[..22].ToUpperInvariant(),
                RouteName = request.DeliveryMethod == "DonorDropOff" ? "Nhận trực tiếp tại kho" : ExtractArea(request.PickupAddress),
                Status = "Planned", CreateAt = DateTime.UtcNow
            };
            context.IntakeBatches.Add(batch);
        }
        else if (batch.ReceivingTeamId != team.Id)
            throw new InvalidOperationException("This shift already belongs to another receiving team.");

        var order = await context.PickupAssignments.Where(x => x.IntakeBatchId == batch.Id && x.IsActive != false)
            .Select(x => (int?)x.RouteOrder).MaxAsync() ?? 0;
        context.PickupAssignments.Add(new PickupAssignment
        {
            Id = Guid.NewGuid(), DonorRequestId = request.Id, ShiftId = team.ShiftId, TeamId = team.Id,
            IntakeBatchId = batch.Id, RouteOrder = order + 1,
            AreaKey = request.DeliveryMethod == "DonorDropOff" ? "Tại kho" : ExtractArea(request.PickupAddress),
            Status = "Pending", CreateAt = DateTime.UtcNow
        });
        request.Status = DonationRequestStatus.ReceivingStaffAssigned;
        request.UpdateAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task<List<ReceivingBatchDto>> GetMyBatchesAsync(Guid staffId)
    {
        var batches = await MyBatchQuery(staffId).OrderByDescending(x => x.IntakeDate).ToListAsync();
        return batches.Select(MapBatch).ToList();
    }

    public async Task<ReceivingBatchDto?> GetMyBatchAsync(Guid staffId, Guid batchId)
    {
        var batch = await MyBatchQuery(staffId).FirstOrDefaultAsync(x => x.Id == batchId);
        return batch is null ? null : MapBatch(batch);
    }

    public async Task StartBatchAsync(Guid staffId, Guid batchId)
    {
        var batch = await RequireMyBatch(staffId, batchId);
        var shift = batch.ReceivingTeam!.Shift;
        if (shift.Status == "Completed") throw new InvalidOperationException("Completed shift cannot be started again.");
        if (shift.Status == "Scheduled")
        {
            shift.Status = "InProgress";
            shift.StartedAt = DateTime.UtcNow;
            shift.UpdateAt = DateTime.UtcNow;
        }
        if (batch.Status == "Planned")
        {
            batch.Status = "Receiving";
            batch.StartedAt = DateTime.UtcNow;
            batch.UpdateAt = DateTime.UtcNow;
        }
        await context.SaveChangesAsync();
    }

    public async Task CompleteShiftAsync(Guid staffId, Guid shiftId)
    {
        var shift = await context.Shifts
            .Include(x => x.Teams).ThenInclude(x => x.Members)
            .Include(x => x.Teams).ThenInclude(x => x.IntakeBatches)
                .ThenInclude(x => x.PickupAssignments)
            .FirstOrDefaultAsync(x => x.Id == shiftId && x.IsActive != false
                && x.Teams.Any(t => t.Members.Any(m => m.StaffId == staffId && m.IsActive != false)))
            ?? throw new InvalidOperationException("Shift not found or is not assigned to this staff member.");

        if (shift.Status != "InProgress")
            throw new InvalidOperationException("Only an in-progress shift can be completed.");

        var batches = shift.Teams
            .Where(t => t.Members.Any(m => m.StaffId == staffId && m.IsActive != false))
            .SelectMany(t => t.IntakeBatches)
            .Where(b => b.IsActive != false)
            .ToList();
        if (batches.SelectMany(b => b.PickupAssignments)
            .Any(a => a.IsActive != false && a.Status == "Pending"))
            throw new InvalidOperationException("All assigned requests must be processed before ending the shift.");

        foreach (var batch in batches.Where(b => b.Status is "Planned" or "Receiving"))
        {
            batch.Status = "Completed";
            batch.CompletedAt = DateTime.UtcNow;
            batch.UpdateAt = DateTime.UtcNow;
        }
        shift.Status = "Completed";
        shift.CompletedAt = DateTime.UtcNow;
        shift.UpdateAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task ConfirmPickupAsync(Guid staffId, Guid batchId, Guid requestId, ConfirmPickupDto dto)
    {
        if (dto.ActualWeight <= 0) throw new InvalidOperationException("Actual weight must be greater than zero.");
        var batch = await RequireMyBatch(staffId, batchId);
        if (batch.Status != "Receiving" || batch.ReceivingTeam?.Shift.Status != "InProgress")
            throw new InvalidOperationException("The assigned shift must be started before receiving donations.");
        var assignment = batch.PickupAssignments.FirstOrDefault(x => x.DonorRequestId == requestId && x.IsActive != false)
            ?? throw new InvalidOperationException("Request is not assigned to this route.");
        if (assignment.Status != "Pending") throw new InvalidOperationException("Request has already been processed.");
        assignment.Status = "Received"; assignment.ProcessedAt = DateTime.UtcNow; assignment.Notes = dto.Notes;
        var alreadyInBatch = await context.IntakeBatchDonationRequests.AnyAsync(x =>
            x.IntakeBatchId == batch.Id && x.DonationRequestId == requestId);
        if (alreadyInBatch) throw new InvalidOperationException("Donation request is already included in this intake batch.");
        context.IntakeBatchDonationRequests.Add(new IntakeBatchDonationRequest
        {
            Id = Guid.NewGuid(), IntakeBatchId = batch.Id, DonationRequestId = requestId,
            AddedAt = DateTime.UtcNow, AddedByStaffId = staffId, CreateAt = DateTime.UtcNow
        });
        assignment.DonorRequest.ActualWeight = dto.ActualWeight;
        assignment.DonorRequest.ImageUrls = dto.ImageUrls ?? assignment.DonorRequest.ImageUrls;
        assignment.DonorRequest.Status = DonationRequestStatus.Confirmed; assignment.DonorRequest.UpdateAt = DateTime.UtcNow;
        batch.TotalWeight += dto.ActualWeight; batch.UpdateAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task RescheduleAsync(Guid staffId, Guid batchId, Guid requestId, ReschedulePickupDto dto)
    {
        var batch = await RequireMyBatch(staffId, batchId);
        EnsureShiftIsInProgress(batch);
        var assignment = RequirePendingAssignment(batch, requestId);
        assignment.Status = "Rescheduled"; assignment.ProcessedAt = DateTime.UtcNow; assignment.Notes = dto.Reason; assignment.IsActive = false;
        assignment.DonorRequest.PickupDate = dto.PickupDate; assignment.DonorRequest.Status = DonationRequestStatus.WaitingReceivingStaff;
        assignment.DonorRequest.UpdateAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task RejectAsync(Guid staffId, Guid batchId, Guid requestId, RejectPickupDto dto)
    {
        var batch = await RequireMyBatch(staffId, batchId);
        EnsureShiftIsInProgress(batch);
        var assignment = RequirePendingAssignment(batch, requestId);
        assignment.Status = "Cancelled"; assignment.ProcessedAt = DateTime.UtcNow; assignment.Notes = dto.Reason;
        assignment.DonorRequest.Status = DonationRequestStatus.Reject; assignment.DonorRequest.RejectReason = dto.Reason;
        assignment.DonorRequest.UpdateAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task CompleteBatchAsync(Guid staffId, Guid batchId)
    {
        var batch = await RequireMyBatch(staffId, batchId);
        if (batch.PickupAssignments.Any(x => x.IsActive != false && x.Status == "Pending"))
            throw new InvalidOperationException("All requests must be processed before completing the batch.");
        batch.Status = "Completed"; batch.CompletedAt = DateTime.UtcNow; batch.UpdateAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task SendToClassificationAsync(Guid staffId, Guid batchId)
    {
        var batch = await RequireMyBatch(staffId, batchId);
        if (batch.Status != "Completed")
            throw new InvalidOperationException("Only a completed intake batch can be sent to classification.");
        if (!batch.IntakeBatchDonationRequests.Any())
            throw new InvalidOperationException("The intake batch does not contain any received donation request.");
        batch.Status = "SentToClassification";
        batch.SentToClassificationAt = DateTime.UtcNow;
        batch.UpdateAt = DateTime.UtcNow;
        batch.UpdatedBy = staffId;
        await context.SaveChangesAsync();
    }

    private IQueryable<IntakeBatch> MyBatchQuery(Guid staffId) => context.IntakeBatches.AsNoTracking()
        .Include(x => x.Warehouse)
        .Include(x => x.ReceivingTeam!).ThenInclude(x => x.Shift)
        .Include(x => x.ReceivingTeam!).ThenInclude(x => x.Members).ThenInclude(x => x.Staff)
        .Include(x => x.PickupAssignments.Where(a => a.IsActive != false)).ThenInclude(x => x.DonorRequest).ThenInclude(x => x.Donor)
        .Where(x => x.IsActive != false && x.ReceivingTeam!.Members.Any(m => m.StaffId == staffId && m.IsActive != false));

    private async Task<IntakeBatch> RequireMyBatch(Guid staffId, Guid batchId) =>
        await context.IntakeBatches.Include(x => x.Warehouse)
            .Include(x => x.ReceivingTeam!).ThenInclude(x => x.Members).ThenInclude(x => x.Staff)
            .Include(x => x.ReceivingTeam!).ThenInclude(x => x.Shift)
            .Include(x => x.PickupAssignments).ThenInclude(x => x.DonorRequest).ThenInclude(x => x.Donor)
            .Include(x => x.IntakeBatchDonationRequests)
            .FirstOrDefaultAsync(x => x.Id == batchId && x.IsActive != false
                && x.ReceivingTeam!.Members.Any(m => m.StaffId == staffId && m.IsActive != false))
        ?? throw new InvalidOperationException("Batch not found or is not assigned to this staff member.");

    private static PickupAssignment RequirePendingAssignment(IntakeBatch batch, Guid requestId)
    {
        var assignment = batch.PickupAssignments.FirstOrDefault(x => x.DonorRequestId == requestId && x.IsActive != false)
            ?? throw new InvalidOperationException("Request is not assigned to this route.");
        if (assignment.Status != "Pending") throw new InvalidOperationException("Request has already been processed.");
        return assignment;
    }

    private static void EnsureShiftIsInProgress(IntakeBatch batch)
    {
        if (batch.Status != "Receiving" || batch.ReceivingTeam?.Shift.Status != "InProgress")
            throw new InvalidOperationException("The assigned shift must be started before processing donations.");
    }

    private static ReceivingBatchDto MapBatch(IntakeBatch batch) => new()
    {
        Id = batch.Id, Code = batch.BatchCode, Route = batch.RouteName, Date = batch.IntakeDate,
        ShiftId = batch.ReceivingTeam?.ShiftId ?? Guid.Empty,
        ShiftStatus = batch.ReceivingTeam?.Shift.Status ?? string.Empty,
        ShiftName = batch.ReceivingTeam?.Shift.ShiftName ?? string.Empty,
        StartTime = batch.ReceivingTeam?.Shift.StartTime ?? default, EndTime = batch.ReceivingTeam?.Shift.EndTime ?? default,
        Status = batch.Status,
        TeamName = batch.ReceivingTeam?.TeamName ?? string.Empty,
        WarehouseAddress = batch.Warehouse?.Address ?? string.Empty,
        TeamMembers = batch.ReceivingTeam?.Members.Where(x => x.IsActive != false)
            .Select(x => new ReceivingTeamMemberDto(x.StaffId, x.Staff.FullName, x.Staff.PhoneNumber)).ToList() ?? [],
        Requests = batch.PickupAssignments.OrderBy(x => x.RouteOrder).Select(x => new ReceivingRequestDto
        {
            Id = x.DonorRequestId, BatchId = batch.Id,
            Code = $"DR-{x.DonorRequest.CreateAt?.Year}-{x.DonorRequestId.ToString()[..8].ToUpperInvariant()}",
            DonorName = x.DonorRequest.ContactName, PhoneNumber = x.DonorRequest.ContactPhoneNumber,
            PickupAddress = x.DonorRequest.PickupAddress, Description = x.DonorRequest.Description ?? string.Empty,
            EstimateWeight = x.DonorRequest.EstimateWeight, ActualWeight = x.DonorRequest.ActualWeight,
            PickupDate = x.DonorRequest.PickupDate, Status = x.Status, Notes = x.Notes,
            DeliveryMethod = x.DonorRequest.DeliveryMethod,
            ImageUrls = x.DonorRequest.ImageUrls
        }).ToList()
    };

    private static string ExtractArea(string address)
    {
        var match = Regex.Match(address, @"(?i)(quận|q\.?|huyện|thành phố|tp\.?|thủ đức)\s*[^,]+", RegexOptions.CultureInvariant);
        return match.Success ? match.Value.Trim() : address.Split(',', StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?.Trim() ?? "Khu vực khác";
    }
}
