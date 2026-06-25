namespace SFA.DAS.FundingRuleValidation.Jobs.Core;

public static class GlobalConstants
{
    public const string FundingRuleCourseAssociationsTableName = "FundingRuleCourseAssociations";
    public const string FundingRulesTableName = "FundingRules";
    public const string IncomingQueueName = "validate-learner";
    public const string OutgoingQueueName = "validate-learner-callback";
    public const string ServiceBusConnectionName = "ServiceBusConnection";
}