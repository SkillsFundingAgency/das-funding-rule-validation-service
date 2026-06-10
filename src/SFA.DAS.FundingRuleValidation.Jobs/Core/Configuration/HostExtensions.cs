using System.Diagnostics.CodeAnalysis;
using Azure.Data.Tables;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.FundingRuleValidation.Jobs.Core.Configuration;

[ExcludeFromCodeCoverage]
public static class HostExtensions
{
    extension(IHost host)
    {
        public IHost InitialiseStorage()
        {
            if (host.Services.GetService(typeof(TableServiceClient)) is not TableServiceClient serviceClient)
            {
                return host;
            }

            var client = serviceClient.GetTableClient("FundingRules");
            client.CreateIfNotExists();
            return host;
        }
    }
}