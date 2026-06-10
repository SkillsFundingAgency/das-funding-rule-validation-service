using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using SFA.DAS.FundingRuleValidation.Jobs.Core.Configuration;

FunctionsApplication
    .CreateBuilder(args)
    .ConfigureFundingRuleValidationApp()
    .Build()
    .InitialiseStorage()
    .Run();