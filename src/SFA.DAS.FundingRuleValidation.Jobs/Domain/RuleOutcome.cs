namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

public record RuleOutcome(Guid RuleId, string RuleName, IEnumerable<FundingRestriction> FundingRestrictions);