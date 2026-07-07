namespace SFA.DAS.FundingRuleValidation.Jobs.Data.Sql;

public class FundingRuleEntity
{
    public required Guid Id { get; set; }
    public required string RuleName { get; set; }
    public required string IlrRuleName { get; set; }
    public required bool Enabled { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }
    public string Parameters { get; set; }
    public List<FundingRuleCourseAssociationsEntity> CourseAssociations { get; set; }
}