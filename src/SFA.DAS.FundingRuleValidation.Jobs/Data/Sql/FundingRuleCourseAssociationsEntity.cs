namespace SFA.DAS.FundingRuleValidation.Jobs.Data.Sql;

public class FundingRuleCourseAssociationsEntity
{
    public FundingRuleEntity FundingRule { get; set; }
    public required Guid FundingRuleId { get; set; }
    public required string CourseId { get; set; }
}