namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

public record RuleOutcome(string RuleName, IEnumerable<FundingRestriction> FundingRestrictions);