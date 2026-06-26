namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

public record FundingRestriction(
    Guid RuleId,
    string RuleName,
    string CourseId,
    string RestrictionName,
    string RestrictedValue);