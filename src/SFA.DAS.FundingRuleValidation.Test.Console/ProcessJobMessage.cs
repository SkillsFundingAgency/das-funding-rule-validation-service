namespace SFA.DAS.FundingRuleValidation.Test.Console;

public record ProcessJobMessage
{
    public long JobId { get; init; }
    public ProcessJobKeyValues KeyValuePairs { get; init; } = default!;
}