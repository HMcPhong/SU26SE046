using BLL.DTOs;
using BLL.Services.Interfaces.ClassificationOperations;
using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Implements.ClassificationOperations;

public class ClassificationOperationsService(AppDbContext context) : IClassificationOperationsService
{
    private const string FabricType = "FabricType";
    private const string GarmentGroup = "GarmentGroup";
    private const string ClothingType = "ClothingType";
    private const string Gender = "Gender";
    private const string TargetUser = "TargetUser";
    private const string Size = "Size";
    private const string ConditionGrade = "ConditionGrade";

    public async Task<IReadOnlyList<ClassificationBatchSummaryDto>> GetBatchesAsync() =>
        await context.IntakeBatches.AsNoTracking()
            .Where(x => x.IsActive != false && (x.Status == "SentToClassification"
                || x.Status == "PendingClassification" || x.Status == "Classifying" || x.Status == "Classified"))
            .OrderByDescending(x => x.IntakeDate)
            .Select(x => new ClassificationBatchSummaryDto(x.Id, x.BatchCode, x.RouteName, x.IntakeDate,
                x.TotalWeight, x.Status == "SentToClassification" ? "PendingConfirmation" : x.Status,
                x.IntakeBatchDonationRequests.Count,
                x.ClassifiedItems.Count(i => i.IsActive != false))).ToListAsync();

    public async Task<ClassificationBatchDetailDto?> GetBatchAsync(Guid batchId)
    {
        var batch = await context.IntakeBatches.AsNoTracking()
            .Include(x => x.IntakeBatchDonationRequests)
            .Include(x => x.ClassifiedItems.Where(i => i.IsActive != false))
            .FirstOrDefaultAsync(x => x.Id == batchId && x.IsActive != false);
        return batch is null ? null : MapBatch(batch);
    }

    public async Task<ClassificationCatalogDto> GetCatalogAsync()
    {
        var categories = await context.Categories.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .ToListAsync();
        IReadOnlyList<CategoryOptionDto> OfType(string type) => categories
            .Where(x => x.Type == type)
            .Select(x => new CategoryOptionDto(x.Id, x.Code, x.Name, x.ParentId, x.SortOrder))
            .ToList();
        var questions = await context.ConditionQuestions.AsNoTracking().Where(x => x.IsActive != false)
            .Include(x => x.Answers.Where(a => a.IsActive != false)).OrderBy(x => x.DisplayOrder).ToListAsync();
        return new ClassificationCatalogDto(OfType(FabricType), OfType(GarmentGroup),
            OfType(ClothingType), OfType(Gender), OfType(TargetUser), OfType(Size), OfType(ConditionGrade),
            questions.Select(q => new ClassificationQuestionDto(q.Id, q.QuestionText, q.DisplayOrder,
                q.Answers.OrderBy(a => a.ConditionRating).Select(a => new ClassificationOptionDto(
                    a.Id, a.AnswerText, Grade(a.ConditionRating))).ToList())).ToList());
    }

    public async Task StartBatchAsync(Guid staffId, Guid batchId)
    {
        var batch = await RequireBatch(batchId);
        if (batch.Status == "Classified") throw new InvalidOperationException("Batch has already been classified.");
        if (batch.Status is not ("PendingClassification" or "Classifying"))
            throw new InvalidOperationException("Confirm receipt of the intake batch before starting classification.");
        batch.Status = "Classifying"; batch.UpdateAt = DateTime.UtcNow; batch.UpdatedBy = staffId;
        await context.SaveChangesAsync();
    }

    public async Task ConfirmReceiptAsync(Guid staffId, Guid batchId)
    {
        var batch = await RequireBatch(batchId);
        if (batch.Status != "SentToClassification")
            throw new InvalidOperationException("Only an intake batch sent by Receiving Staff can be confirmed.");
        batch.Status = "PendingClassification";
        batch.ClassificationReceivedAt = DateTime.UtcNow;
        batch.ClassificationReceivedByStaffId = staffId;
        batch.UpdateAt = DateTime.UtcNow;
        batch.UpdatedBy = staffId;
        await context.SaveChangesAsync();
    }

    public async Task<ClassificationItemDto> ClassifyItemAsync(Guid staffId, Guid batchId, ClassifyItemDto dto)
    {
        var batch = await RequireBatch(batchId);
        if (batch.Status != "Classifying") throw new InvalidOperationException("Start the batch before classifying items.");
        var categorySelection = await ResolveCategoriesAsync(dto);
        var questions = await context.ConditionQuestions.Include(x => x.Answers)
            .Where(x => x.IsActive != false).OrderBy(x => x.DisplayOrder).ToListAsync();
        if (dto.Answers.Count != questions.Count || dto.Answers.Select(x => x.QuestionId).Distinct().Count() != questions.Count)
            throw new InvalidOperationException("Every condition question must be answered exactly once.");
        var ratings = new List<int>();
        foreach (var question in questions)
        {
            var selected = dto.Answers.SingleOrDefault(x => x.QuestionId == question.Id);
            var answer = question.Answers.FirstOrDefault(x => x.Id == selected?.AnswerId && x.IsActive != false)
                ?? throw new InvalidOperationException("An answer does not belong to its condition question.");
            ratings.Add(answer.ConditionRating);
        }
        var rating = ratings.Contains(3) ? 3 : ratings.Count(x => x == 2) >= 2 ? 2 : 1;
        var grade = await context.Categories.FirstOrDefaultAsync(x => x.Type == ConditionGrade
            && x.Code == $"GRADE_{Grade(rating)}" && x.IsActive != false)
            ?? throw new InvalidOperationException("The condition grade category is not configured.");
        var item = new ClassifiedItem
        {
            Id = Guid.NewGuid(), BatchId = batchId, ItemCode = $"CI-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant(),
            FabricTypeId = categorySelection.Fabric.Id, GarmentGroupId = categorySelection.Group.Id,
            ClothingTypeId = categorySelection.Clothing.Id, GenderId = categorySelection.Gender.Id,
            TargetUserId = categorySelection.Target.Id, SizeId = categorySelection.Size.Id, ConditionGradeId = grade.Id,
            FabricType = categorySelection.Fabric.Name, GarmentGroup = categorySelection.Group.Name,
            ClothingType = categorySelection.Clothing.Name, Gender = categorySelection.Gender.Name,
            TargetUser = categorySelection.Target.Name, Size = categorySelection.Size.Name, ConditionRating = rating,
            ProcessingDirection = rating == 1 ? "Charity" : rating == 2 ? "Recycling" : "Disposal",
            ImageUrls = dto.ImageUrls, Notes = dto.Notes, ClassifiedByStaffId = staffId, ClassifiedAt = DateTime.UtcNow,
            CreateAt = DateTime.UtcNow, CreatedBy = staffId
        };
        var groupedBatch = await GetOrCreateGroupedBatchAsync(batch, item, staffId);
        item.ClassifiedBatchId = groupedBatch.Id;
        await LinkBatchProvenanceAsync(groupedBatch.Id, batchId, staffId);
        groupedBatch.TotalItem++;
        groupedBatch.UpdateAt = DateTime.UtcNow;
        groupedBatch.UpdatedBy = staffId;
        context.ClassifiedItems.Add(item);
        context.InspectionAnswers.AddRange(dto.Answers.Select(x => new InspectionAnswer
        {
            Id = Guid.NewGuid(), ClassifiedItemId = item.Id, ConditionQuestionId = x.QuestionId,
            ConditionAnswerId = x.AnswerId, CreateAt = DateTime.UtcNow, CreatedBy = staffId
        }));
        await context.SaveChangesAsync();
        return MapItem(item);
    }

    public async Task<IReadOnlyList<GroupedClassifiedBatchDto>> GetGroupedBatchesAsync(DateTime? date)
    {
        var query = context.ClassifiedBatches.AsNoTracking().Where(x => x.IsActive != false);
        if (date.HasValue) query = query.Where(x => x.ClassificationDate == date.Value.Date);
        return await query.OrderByDescending(x => x.ClassificationDate).ThenBy(x => x.BatchCode)
            .Select(x => new GroupedClassifiedBatchDto(x.Id, x.BatchCode, x.ClassificationDate,
                x.FabricType, x.GarmentGroup, x.ClothingType, x.Gender, x.TargetUser, x.Size,
                x.ConditionRating == 1 ? "A" : x.ConditionRating == 2 ? "B" : "C",
                x.ProcessingDirection, x.TotalItem, x.Status,
                x.DonationRequestSources.Where(s => s.IsActive != false)
                    .Select(s => s.DonationRequest.RequestCode).Distinct().OrderBy(code => code).ToList()))
            .ToListAsync();
    }

    public async Task<GroupedClassifiedBatchDetailDto?> GetGroupedBatchAsync(Guid groupedBatchId)
    {
        var group = await context.ClassifiedBatches.AsNoTracking()
            .Include(x => x.Items.Where(i => i.IsActive != false))
            .Include(x => x.DonationRequestSources.Where(s => s.IsActive != false))
                .ThenInclude(x => x.DonationRequest)
            .FirstOrDefaultAsync(x => x.Id == groupedBatchId && x.IsActive != false);
        return group is null ? null : new GroupedClassifiedBatchDetailDto(group.Id, group.BatchCode,
            group.ClassificationDate, group.FabricType, group.GarmentGroup, group.ClothingType,
            group.Gender, group.TargetUser, group.Size, Grade(group.ConditionRating),
            group.ProcessingDirection, group.TotalItem, group.Status,
            group.DonationRequestSources.Select(x => x.DonationRequest.RequestCode)
                .Distinct().OrderBy(code => code).ToList(),
            group.Items.OrderBy(x => x.ClassifiedAt).Select(MapItem).ToList());
    }

    public async Task SendGroupedBatchToWarehouseAsync(Guid staffId, Guid groupedBatchId)
    {
        var batch = await context.ClassifiedBatches
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == groupedBatchId && x.IsActive != false)
            ?? throw new InvalidOperationException("Classified batch not found.");
        if (batch.Status != "Open")
            throw new InvalidOperationException("Only an open classified batch can be sent to warehouse.");
        if (!batch.Items.Any(x => x.IsActive != false))
            throw new InvalidOperationException("The classified batch does not contain any item.");

        batch.TotalItem = batch.Items.Count(x => x.IsActive != false);
        batch.Status = "PendingWarehouseReceipt";
        batch.SentToWarehouseAt = DateTime.UtcNow;
        batch.SentToWarehouseByStaffId = staffId;
        batch.UpdateAt = DateTime.UtcNow;
        batch.UpdatedBy = staffId;
        await context.SaveChangesAsync();
    }

    public async Task<SendGroupedBatchesToWarehouseResultDto> SendGroupedBatchesToWarehouseAsync(
        Guid staffId, IReadOnlyList<Guid> groupedBatchIds)
    {
        var ids = groupedBatchIds.Where(x => x != Guid.Empty).Distinct().ToList();
        if (ids.Count == 0)
            throw new InvalidOperationException("Select at least one classified batch.");

        var batches = await context.ClassifiedBatches
            .Include(x => x.Items)
            .Where(x => ids.Contains(x.Id) && x.IsActive != false)
            .ToListAsync();
        if (batches.Count != ids.Count)
            throw new InvalidOperationException("One or more classified batches no longer exist.");

        var now = DateTime.UtcNow;
        var sent = 0;
        foreach (var batch in batches.Where(x => x.Status == "Open"))
        {
            var itemCount = batch.Items.Count(x => x.IsActive != false);
            if (itemCount == 0)
                throw new InvalidOperationException(
                    $"Classified batch {batch.BatchCode} does not contain any item.");

            batch.TotalItem = itemCount;
            batch.Status = "PendingWarehouseReceipt";
            batch.SentToWarehouseAt = now;
            batch.SentToWarehouseByStaffId = staffId;
            batch.UpdateAt = now;
            batch.UpdatedBy = staffId;
            sent++;
        }

        if (sent > 0) await context.SaveChangesAsync();
        return new SendGroupedBatchesToWarehouseResultDto(sent, batches.Count - sent);
    }

    public async Task CompleteBatchAsync(Guid staffId, Guid batchId)
    {
        var batch = await RequireBatch(batchId);
        if (batch.Status != "Classifying") throw new InvalidOperationException("Only a batch being classified can be completed.");
        if (!await context.ClassifiedItems.AnyAsync(x => x.BatchId == batchId && x.IsActive != false))
            throw new InvalidOperationException("Classify at least one item before completing the batch.");
        batch.Status = "Classified"; batch.UpdateAt = DateTime.UtcNow; batch.UpdatedBy = staffId;
        await context.SaveChangesAsync();
    }

    private async Task<IntakeBatch> RequireBatch(Guid id) => await context.IntakeBatches
        .FirstOrDefaultAsync(x => x.Id == id && x.IsActive != false)
        ?? throw new InvalidOperationException("Intake batch not found.");

    private async Task<ClassifiedBatch> GetOrCreateGroupedBatchAsync(IntakeBatch intakeBatch,
        ClassifiedItem item, Guid staffId)
    {
        var localDate = DateTime.UtcNow.AddHours(7).Date;
        var key = string.Join('|', intakeBatch.WarehouseId, localDate.ToString("yyyyMMdd"),
            item.ConditionGradeId, item.FabricTypeId, item.GarmentGroupId, item.ClothingTypeId,
            item.GenderId, item.TargetUserId, item.SizeId, item.ProcessingDirection.ToLowerInvariant());
        var group = await context.ClassifiedBatches.FirstOrDefaultAsync(x => x.GroupKey == key && x.IsActive != false);
        if (group is not null) return group;
        group = new ClassifiedBatch
        {
            Id = Guid.NewGuid(), WarehouseId = intakeBatch.WarehouseId, ClassificationDate = localDate,
            GroupKey = key, BatchCode = $"CB-{localDate:yyyyMMdd}-{Grade(item.ConditionRating)}-{Guid.NewGuid():N}"[..24].ToUpperInvariant(),
            FabricTypeId = item.FabricTypeId, GarmentGroupId = item.GarmentGroupId,
            ClothingTypeId = item.ClothingTypeId, GenderId = item.GenderId,
            TargetUserId = item.TargetUserId, SizeId = item.SizeId, ConditionGradeId = item.ConditionGradeId,
            FabricType = item.FabricType, GarmentGroup = item.GarmentGroup, ClothingType = item.ClothingType,
            Gender = item.Gender, TargetUser = item.TargetUser, Size = item.Size,
            ConditionRating = item.ConditionRating, ProcessingDirection = item.ProcessingDirection,
            Status = "Open", TotalItem = 0, TotalWeight = 0, CreateAt = DateTime.UtcNow,
            CreatedBy = staffId
        };
        context.ClassifiedBatches.Add(group);
        return group;
    }

    private async Task LinkBatchProvenanceAsync(Guid classifiedBatchId, Guid intakeBatchId, Guid staffId)
    {
        var requestIds = await context.IntakeBatchDonationRequests.AsNoTracking()
            .Where(x => x.IntakeBatchId == intakeBatchId && x.IsActive != false)
            .Select(x => x.DonationRequestId)
            .Distinct()
            .ToListAsync();
        if (requestIds.Count == 0) return;

        var existingIds = await context.ClassifiedBatchDonationRequests.AsNoTracking()
            .Where(x => x.ClassifiedBatchId == classifiedBatchId
                && x.IntakeBatchId == intakeBatchId
                && requestIds.Contains(x.DonationRequestId))
            .Select(x => x.DonationRequestId)
            .ToListAsync();
        var now = DateTime.UtcNow;
        context.ClassifiedBatchDonationRequests.AddRange(requestIds
            .Where(id => !existingIds.Contains(id))
            .Select(id => new ClassifiedBatchDonationRequest
            {
                Id = Guid.NewGuid(),
                ClassifiedBatchId = classifiedBatchId,
                DonationRequestId = id,
                IntakeBatchId = intakeBatchId,
                LinkedAt = now,
                CreateAt = now,
                CreatedBy = staffId,
                IsActive = true
            }));
    }

    private async Task<CategorySelection> ResolveCategoriesAsync(ClassifyItemDto dto)
    {
        var ids = new[] { dto.FabricTypeId, dto.GarmentGroupId, dto.ClothingTypeId,
            dto.GenderId, dto.TargetUserId, dto.SizeId };
        if (ids.Any(x => x == Guid.Empty) || ids.Distinct().Count() != ids.Length)
            throw new InvalidOperationException("Every classification category must be selected.");
        var values = await context.Categories.Where(x => ids.Contains(x.Id) && x.IsActive != false).ToListAsync();
        Category Require(Guid id, string type) => values.FirstOrDefault(x => x.Id == id && x.Type == type)
            ?? throw new InvalidOperationException($"The selected {type} category is invalid or inactive.");
        var result = new CategorySelection(Require(dto.FabricTypeId, FabricType),
            Require(dto.GarmentGroupId, GarmentGroup), Require(dto.ClothingTypeId, ClothingType),
            Require(dto.GenderId, Gender), Require(dto.TargetUserId, TargetUser), Require(dto.SizeId, Size));
        if (result.Clothing.ParentId != result.Group.Id)
            throw new InvalidOperationException("The clothing type does not belong to the selected garment group.");
        return result;
    }

    private sealed record CategorySelection(Category Fabric, Category Group, Category Clothing,
        Category Gender, Category Target, Category Size);

    private static string NormalizeStatus(string status) => status switch
    { "SentToClassification" => "PendingConfirmation", _ => status };
    private static string Grade(int rating) => rating == 1 ? "A" : rating == 2 ? "B" : "C";
    private static ClassificationItemDto MapItem(ClassifiedItem x) => new(x.Id, x.ItemCode, x.FabricType,
        x.GarmentGroup, x.ClothingType, x.Gender, x.TargetUser, x.Size, Grade(x.ConditionRating),
        x.ProcessingDirection, x.ImageUrls ?? [], x.Notes, x.ClassifiedAt);
    private static ClassificationBatchDetailDto MapBatch(IntakeBatch x) => new(x.Id, x.BatchCode, x.RouteName,
        x.IntakeDate, x.TotalWeight, NormalizeStatus(x.Status), x.IntakeBatchDonationRequests.Count,
        x.ClassifiedItems.OrderBy(i => i.ClassifiedAt).Select(MapItem).ToList());
}
