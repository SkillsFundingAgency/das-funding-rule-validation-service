namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

public record FundingRestriction(string RestrictionName, string RestrictedValue)
{
    public static FundingRestriction Unknown => new("Unknown", "Unknown");
}