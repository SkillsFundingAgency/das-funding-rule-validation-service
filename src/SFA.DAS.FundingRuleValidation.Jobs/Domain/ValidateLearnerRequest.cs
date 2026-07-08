namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

public record ValidateLearnerCommand(string CorrelationId, string Ukprn, string Uln, IEnumerable<Course> Courses);