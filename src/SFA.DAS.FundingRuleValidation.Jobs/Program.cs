using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using SFA.DAS.FundingRuleValidation.Jobs.Core.Configuration;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFundingRuleValidationApp();

builder.Build().Run();