namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

public class FundingRule
{
    public required Guid Id { get; set; }
    public required string RuleName { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string Parameters { get; set; }
    public List<string> CourseIds { get; set; }
}