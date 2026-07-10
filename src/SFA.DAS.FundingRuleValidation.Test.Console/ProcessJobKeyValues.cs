namespace SFA.DAS.FundingRuleValidation.Test.Console;

public record ProcessJobKeyValues
{
    public string Ukprn { get; init; } = default!;
    public string Container { get; init; } = default!;
    public string Filename { get; init; } = default!;
}