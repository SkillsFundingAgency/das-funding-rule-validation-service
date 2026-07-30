namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

public record ValidateLearnerResult(Guid CorrelationId, long Ukprn, long Uln, ValidationStatus Status, IEnumerable<RuleOutcome> RuleOutcomes);