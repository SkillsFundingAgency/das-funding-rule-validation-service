namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

public record ValidateLearnerCommand(string CorrelationId, long Ukprn, long Uln, IEnumerable<Course> Courses);