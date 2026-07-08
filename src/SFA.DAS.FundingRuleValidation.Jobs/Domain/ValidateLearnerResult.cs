namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

public record ValidateLearnerResult(string CorrelationId, string Ukprn, string Uln, ValidationStatus Status, IEnumerable<RuleOutcome> RuleOutcomes);