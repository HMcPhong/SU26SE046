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
    public string FabricType { get; set; } = string.Empty;
    public string GarmentGroup { get; set; } = string.Empty;
    public string ClothingType { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string TargetUser { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = [];
    public string? Notes { get; set; }
    public List<ClassificationAnswerDto> Answers { get; set; } = [];
}

public record ClassificationOptionDto(Guid Id, string Text, string Grade);
public record ClassificationQuestionDto(Guid Id, string Text, int DisplayOrder,
    IReadOnlyList<ClassificationOptionDto> Options);
public record ClassificationCatalogDto(IReadOnlyList<string> FabricTypes,
    IReadOnlyDictionary<string, IReadOnlyList<string>> ClothingTypes,
    IReadOnlyList<string> Genders, IReadOnlyList<string> TargetUsers,
    IReadOnlyList<string> Sizes, IReadOnlyList<ClassificationQuestionDto> ConditionQuestions);

public record GroupedClassifiedBatchDto(Guid Id, string BatchCode, DateTime ClassificationDate,
    string FabricType, string GarmentGroup, string ClothingType, string Gender, string TargetUser,
    string Size, string ConditionGrade, string ProcessingDirection, int TotalItem, string Status);

public record GroupedClassifiedBatchDetailDto(Guid Id, string BatchCode, DateTime ClassificationDate,
    string FabricType, string GarmentGroup, string ClothingType, string Gender, string TargetUser,
    string Size, string ConditionGrade, string ProcessingDirection, int TotalItem, string Status,
    IReadOnlyList<ClassificationItemDto> Items);
