using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.FundingRuleValidation.Jobs.Core.Configuration;

[ExcludeFromCodeCoverage]
public class ConnectionStringsConfiguration
{
    public string? SqlConnectionString { get; set; }
    public string? TableStorageConnectionString { get; set; }
}