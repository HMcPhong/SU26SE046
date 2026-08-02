namespace BLL.DTOs;

public record ConditionAnswerConfigDto(Guid Id, string Text, string Grade);
public record ConditionQuestionConfigDto(Guid Id, string QuestionText, int DisplayOrder,
    IReadOnlyList<ConditionAnswerConfigDto> Answers);

public class SaveConditionQuestionConfigDto
{
    public string QuestionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public string AnswerA { get; set; } = string.Empty;
    public string AnswerB { get; set; } = string.Empty;
    public string AnswerC { get; set; } = string.Empty;
}
