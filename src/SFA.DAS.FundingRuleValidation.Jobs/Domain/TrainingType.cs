using System.Text.Json.Serialization;

namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TrainingType
{
    Standard,    // ProgType 25
    ShortCourse  // all other ProgTypes
}