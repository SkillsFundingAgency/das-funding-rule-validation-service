namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

public record FundingRestriction(
    string CourseId,
    int AimSequenceNumber,
    string RestrictionName,
    string RestrictedValue);