using System.Diagnostics.CodeAnalysis;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.FundingRuleValidation.Jobs.Data;
using SFA.DAS.FundingRuleValidation.Jobs.Data.TableStorage;

namespace SFA.DAS.FundingRuleValidation.Jobs.Core.Configuration;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    extension(FunctionsApplicationBuilder builder)
    {
        public FunctionsApplicationBuilder ConfigureFundingRuleValidationApp()
        {
            return builder
                .ConfigureFunctionsWebApplication()
                .RegisterConfiguration()
                .RegisterServices()
                .RegisterDependencies();
        }

        private FunctionsApplicationBuilder RegisterConfiguration()
        {
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", optional: true);

            builder.Configuration.AddAzureTableStorageConfiguration();

            // add custom configuration sections
            builder.Services.Configure<ConnectionStringsConfiguration>(builder.Configuration.GetSection("ConnectionStrings"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<ConnectionStringsConfiguration>>()!.Value);

            return builder;
        }

        private FunctionsApplicationBuilder RegisterServices()
        {
            builder.Services.AddOpenTelemetryRegistration(builder.Configuration.GetValue<string>("APPLICATIONINSIGHTS_CONNECTION_STRING"));
            builder.Services.AddTransient(cfg =>
            {
                var config = cfg.GetService<IOptions<ConnectionStringsConfiguration>>()?.Value;
                return new TableServiceClient(config?.TableStorageConnectionString);
            });
            
            return builder;
        }

        private FunctionsApplicationBuilder RegisterDependencies()
        {
            var services = builder.Services;
            services.AddTransient<IRulesRepository, TableStorageRulesRepository>();
        
            return builder;
        }
    }
}