namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

public record RuleCourseOutcome(Guid RuleId, string RuleName, string RuleDescription, string CourseId, int AimSequenceNumber, RuleOutcome Outcome, IEnumerable<FundingRestriction> FundingRestrictions);