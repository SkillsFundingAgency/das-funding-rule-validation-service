using System.Text.Json.Serialization;

namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RuleOutcome
{
    Error,
    Success,
    Warning,
}