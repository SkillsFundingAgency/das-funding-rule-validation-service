namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

public record ValidateLearnerCommand(Guid CorrelationId, long Ukprn, long Uln, IEnumerable<Course> Courses);