using Microsoft.DurableTask;

namespace SFA.DAS.FundingRuleValidation.Jobs.Core;

public static class GlobalConstants
{
    public const string FundingRuleCourseAssociationsTableName = "FundingRuleCourseAssociations";
    public const string FundingRulesTableName = "FundingRules";
    public const string IncomingQueueName = "validate-learner-requests";
    public const string OutgoingQueueName = "validate-learner-callback";
    public const string ServiceBusConnectionName = "ServiceBusConnection";
    
    private const int MaxRetryCount = 5;
    private const int FirstRetryIntervalInMilliseconds = 50;
    private static readonly RetryPolicy RetryPolicy = new(MaxRetryCount, TimeSpan.FromMilliseconds(FirstRetryIntervalInMilliseconds));
    private static readonly TaskRetryOptions TaskRetryOptions = new(RetryPolicy);
    public static readonly TaskOptions TaskOptions = new(TaskRetryOptions);
}