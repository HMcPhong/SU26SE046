using BLL.DTOs;
using BLL.Services.Interfaces.ClassificationOperations;
using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Implements.ClassificationOperations;

public class ClassificationOperationsService(AppDbContext context) : IClassificationOperationsService
{
    private static readonly string[] Fabrics = ["Vải cotton", "Vải lanh", "Vải lụa", "Vải len", "Vải nylon", "Vải dù", "Da", "Vải jean"];
    private static readonly Dictionary<string, IReadOnlyList<string>> Clothes = new()
    {
        ["Áo"] = ["Áo phông tay ngắn", "Áo phông tay dài", "Áo ba lỗ", "Áo sơ mi tay ngắn", "Áo sơ mi tay dài", "Áo khoác", "Áo vest", "Áo blazer", "Áo sweater", "Áo polo", "Áo dài"],
        ["Quần"] = ["Quần tây", "Quần ngắn", "Quần kaki", "Quần dài", "Quần ống rộng", "Váy"]
    };

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
        var questions = await context.ConditionQuestions.AsNoTracking().Where(x => x.IsActive != false)
            .Include(x => x.Answers.Where(a => a.IsActive != false)).OrderBy(x => x.DisplayOrder).ToListAsync();
        return new ClassificationCatalogDto(Fabrics, Clothes,
            ["Nam", "Nữ", "Unisex"], ["Em bé", "Trẻ em", "Người lớn"],
            ["S", "M", "L", "XL", "XXL", "XXXL", "Freesize"],
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
        ValidateAttributes(dto);
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
        var item = new ClassifiedItem
        {
            Id = Guid.NewGuid(), BatchId = batchId, ItemCode = $"CI-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant(),
            FabricType = dto.FabricType, GarmentGroup = dto.GarmentGroup, ClothingType = dto.ClothingType,
            Gender = dto.Gender, TargetUser = dto.TargetUser, Size = dto.Size, ConditionRating = rating,
            ProcessingDirection = rating == 1 ? "Charity" : rating == 2 ? "Recycling" : "Disposal",
            ImageUrls = dto.ImageUrls, Notes = dto.Notes, ClassifiedByStaffId = staffId, ClassifiedAt = DateTime.UtcNow,
            CreateAt = DateTime.UtcNow, CreatedBy = staffId
        };
        var groupedBatch = await GetOrCreateGroupedBatchAsync(batch, item, staffId);
        item.ClassifiedBatchId = groupedBatch.Id;
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
                x.ProcessingDirection, x.TotalItem, x.Status)).ToListAsync();
    }

    public async Task<GroupedClassifiedBatchDetailDto?> GetGroupedBatchAsync(Guid groupedBatchId)
    {
        var group = await context.ClassifiedBatches.AsNoTracking()
            .Include(x => x.Items.Where(i => i.IsActive != false))
            .FirstOrDefaultAsync(x => x.Id == groupedBatchId && x.IsActive != false);
        return group is null ? null : new GroupedClassifiedBatchDetailDto(group.Id, group.BatchCode,
            group.ClassificationDate, group.FabricType, group.GarmentGroup, group.ClothingType,
            group.Gender, group.TargetUser, group.Size, Grade(group.ConditionRating),
            group.ProcessingDirection, group.TotalItem, group.Status,
            group.Items.OrderBy(x => x.ClassifiedAt).Select(MapItem).ToList());
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
            item.ConditionRating, item.FabricType.Trim().ToLowerInvariant(),
            item.GarmentGroup.Trim().ToLowerInvariant(), item.ClothingType.Trim().ToLowerInvariant(),
            item.Gender.Trim().ToLowerInvariant(), item.TargetUser.Trim().ToLowerInvariant(),
            item.Size.Trim().ToLowerInvariant(), item.ProcessingDirection.ToLowerInvariant());
        var group = await context.ClassifiedBatches.FirstOrDefaultAsync(x => x.GroupKey == key && x.IsActive != false);
        if (group is not null) return group;
        group = new ClassifiedBatch
        {
            Id = Guid.NewGuid(), WarehouseId = intakeBatch.WarehouseId, ClassificationDate = localDate,
            GroupKey = key, BatchCode = $"CB-{localDate:yyyyMMdd}-{Grade(item.ConditionRating)}-{Guid.NewGuid():N}"[..24].ToUpperInvariant(),
            FabricType = item.FabricType, GarmentGroup = item.GarmentGroup, ClothingType = item.ClothingType,
            Gender = item.Gender, TargetUser = item.TargetUser, Size = item.Size,
            ConditionRating = item.ConditionRating, ProcessingDirection = item.ProcessingDirection,
            Status = "Open", TotalItem = 0, TotalWeight = 0, CreateAt = DateTime.UtcNow,
            CreatedBy = staffId
        };
        context.ClassifiedBatches.Add(group);
        return group;
    }

    private static void ValidateAttributes(ClassifyItemDto dto)
    {
        if (!Fabrics.Contains(dto.FabricType) || !Clothes.TryGetValue(dto.GarmentGroup, out var types)
            || !types.Contains(dto.ClothingType) || !new[] { "Nam", "Nữ", "Unisex" }.Contains(dto.Gender)
            || !new[] { "Em bé", "Trẻ em", "Người lớn" }.Contains(dto.TargetUser)
            || !new[] { "S", "M", "L", "XL", "XXL", "XXXL", "Freesize" }.Contains(dto.Size))
            throw new InvalidOperationException("One or more item attributes are invalid.");
    }

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
