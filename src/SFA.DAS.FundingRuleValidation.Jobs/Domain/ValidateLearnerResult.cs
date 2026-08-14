namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

public record ValidateLearnerResult(string CorrelationId, string WaitingInstanceId, string Ukprn, string Uln, ValidationStatus Status, IEnumerable<RuleCourseOutcome> RuleOutcomes);