using Azure;
using Azure.Data.Tables;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data.TableStorage;

public class FundingRule : ITableEntity
{
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public required string RuleName { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}