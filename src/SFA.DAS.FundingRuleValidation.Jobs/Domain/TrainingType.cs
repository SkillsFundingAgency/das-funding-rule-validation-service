using System.Text.Json.Serialization;

namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TrainingType
{
    Apprenticeship,
    FoundationApprenticeship,
    MathsAndEnglish,
    ApprenticeshipUnit,
}