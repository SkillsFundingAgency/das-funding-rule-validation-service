using Azure;
using Azure.Data.Tables;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data.TableStorage;

public class FundingRuleCourseAssociationTableEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}