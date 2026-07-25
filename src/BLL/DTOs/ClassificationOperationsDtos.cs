namespace BLL.DTOs;

public record ClassificationBatchSummaryDto(Guid Id, string BatchCode, string RouteName,
    DateTime IntakeDate, decimal TotalWeight, string Status, int DonationRequests, int ClassifiedItems);

public record ClassificationItemDto(Guid Id, string ItemCode, string FabricType, string GarmentGroup,
    string ClothingType, string Gender, string TargetUser, string Size, string ConditionGrade,
    string ProcessingDirection, IReadOnlyList<string> ImageUrls, string? Notes, DateTime ClassifiedAt);

public record ClassificationBatchDetailDto(Guid Id, string BatchCode, string RouteName,
    DateTime IntakeDate, decimal TotalWeight, string Status, int DonationRequests,
    IReadOnlyList<ClassificationItemDto> Items);

public record ClassificationAnswerDto(Guid QuestionId, Guid AnswerId);

public class ClassifyItemDto
{
    public Guid FabricTypeId { get; set; }
    public Guid GarmentGroupId { get; set; }
    public Guid ClothingTypeId { get; set; }
    public Guid GenderId { get; set; }
    public Guid TargetUserId { get; set; }
    public Guid SizeId { get; set; }
    public List<string> ImageUrls { get; set; } = [];
    public string? Notes { get; set; }
    public List<ClassificationAnswerDto> Answers { get; set; } = [];
}

public record ClassificationOptionDto(Guid Id, string Text, string Grade);
public record ClassificationQuestionDto(Guid Id, string Text, int DisplayOrder,
    IReadOnlyList<ClassificationOptionDto> Options);
public record CategoryOptionDto(Guid Id, string Code, string Name, Guid? ParentId, int SortOrder);
public record ClassificationCatalogDto(IReadOnlyList<CategoryOptionDto> FabricTypes,
    IReadOnlyList<CategoryOptionDto> GarmentGroups, IReadOnlyList<CategoryOptionDto> ClothingTypes,
    IReadOnlyList<CategoryOptionDto> Genders, IReadOnlyList<CategoryOptionDto> TargetUsers,
    IReadOnlyList<CategoryOptionDto> Sizes, IReadOnlyList<CategoryOptionDto> ConditionGrades,
    IReadOnlyList<ClassificationQuestionDto> ConditionQuestions);

public record GroupedClassifiedBatchDto(Guid Id, string BatchCode, DateTime ClassificationDate,
    string FabricType, string GarmentGroup, string ClothingType, string Gender, string TargetUser,
    string Size, string ConditionGrade, string ProcessingDirection, int TotalItem, string Status);

public record GroupedClassifiedBatchDetailDto(Guid Id, string BatchCode, DateTime ClassificationDate,
    string FabricType, string GarmentGroup, string ClothingType, string Gender, string TargetUser,
    string Size, string ConditionGrade, string ProcessingDirection, int TotalItem, string Status,
    IReadOnlyList<ClassificationItemDto> Items);
