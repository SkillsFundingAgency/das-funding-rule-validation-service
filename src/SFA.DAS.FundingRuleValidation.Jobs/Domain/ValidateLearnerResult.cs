namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

public record ValidateLearnerResult(string CorrelationId, long Ukprn, long Uln, ValidationStatus Status, IEnumerable<RuleOutcome> RuleOutcomes);