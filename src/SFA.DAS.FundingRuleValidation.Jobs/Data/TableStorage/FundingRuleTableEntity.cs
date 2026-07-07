using Azure;
using Azure.Data.Tables;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data.TableStorage;

public class FundingRuleTableEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string RuleName { get; set; }
    public string IlrRuleName { get; set; }
    public bool Enabled { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }
    public string Parameters { get; set; }
    public string Courses { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}