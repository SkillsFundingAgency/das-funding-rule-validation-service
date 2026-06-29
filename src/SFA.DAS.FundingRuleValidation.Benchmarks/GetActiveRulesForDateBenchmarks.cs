using System.Text.Json;
using Azure.Data.Tables;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SFA.DAS.FundingRuleValidation.Jobs.Activities;
using SFA.DAS.FundingRuleValidation.Jobs.Activities.Config;
using SFA.DAS.FundingRuleValidation.Jobs.Core;
using SFA.DAS.FundingRuleValidation.Jobs.Data.Sql;
using SFA.DAS.FundingRuleValidation.Jobs.Data.TableStorage;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
[CategoriesColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class GetActiveRulesForDateBenchmarks
{
    private SqlRulesRepository _sqlRulesRepository = null!;
    private TableStorageRulesRepository _tableStorageRulesRepository = null!;
    private FundingRulesDbContext _dbContext = null!;
    private TableServiceClient _tableServiceClient = null!;

    private readonly List<DateTime> _dates = [
        DateTime.UtcNow.Date.AddDays(-10).Date,
        DateTime.UtcNow.Date,
        DateTime.UtcNow.Date.AddDays(10).Date
    ];

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>()
            .Build();

        var sqlConnectionString = configuration.GetConnectionString("SqlConnectionString");
        var tableStorageConnectionString = configuration.GetConnectionString("TableStorageConnectionString");

        if (string.IsNullOrWhiteSpace(sqlConnectionString))
        {
            throw new InvalidOperationException("Missing ConnectionStrings:SqlConnectionString.");
        }

        if (string.IsNullOrWhiteSpace(tableStorageConnectionString))
        {
            throw new InvalidOperationException("Missing ConnectionStrings:TableStorageConnectionString.");
        }

        var dbContextOptions = new DbContextOptionsBuilder<FundingRulesDbContext>()
            .UseSqlServer(sqlConnectionString)
            .Options;

        _dbContext = new FundingRulesDbContext(dbContextOptions);

        _tableServiceClient = new TableServiceClient(tableStorageConnectionString);

        _sqlRulesRepository = new SqlRulesRepository(_dbContext);
        _tableStorageRulesRepository = new TableStorageRulesRepository(_tableServiceClient);

        await InitialiseData();
        await WarmUpRepositories();
    }

    [BenchmarkCategory("GetActiveRulesForDate")]
    [Benchmark(Baseline = true)]
    public async Task<List<FundingRule>> SqlServer()
    {
        return await _sqlRulesRepository.GetActiveRulesForDatesAsync(_dates);
    }

    [BenchmarkCategory("GetActiveRulesForDate")]
    [Benchmark]
    public async Task<List<FundingRule>> AzureTableStorage()
    {
        return await _tableStorageRulesRepository.GetActiveRulesForDatesAsync(_dates);
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await _dbContext.DisposeAsync();
    }

    private static List<FundingRuleCourseAssociationsEntity> GenerateSqlCourseAssociations(Guid fundingRuleId) 
        => Enumerable
                .Range(1, Random.Shared.Next(1, 200))
                .Select(_ => new FundingRuleCourseAssociationsEntity
                {
                    FundingRuleId = fundingRuleId,
                    CourseId = Guid.NewGuid().ToString(),
                })
                .ToList();

    private async Task InitialiseData()
    {
        // delete data
        await _dbContext.CourseAssociations.ExecuteDeleteAsync();
        await _dbContext.FundingRules.ExecuteDeleteAsync();
        
        var fundingRuleClient = _tableServiceClient.GetTableClient(GlobalConstants.FundingRulesTableName);
        await fundingRuleClient.CreateIfNotExistsAsync();
        await DeleteAllTableStorageRowsAsync(fundingRuleClient);
        
        var courseAssociationsClient = _tableServiceClient.GetTableClient(GlobalConstants.FundingRuleCourseAssociationsTableName);
        await courseAssociationsClient.CreateIfNotExistsAsync();
        await DeleteAllTableStorageRowsAsync(courseAssociationsClient);

        // add data
        for (var index = 0; index < 500; index++)
        {
            var fundingRuleId = Guid.NewGuid();
            var sqlCourseAssociations = GenerateSqlCourseAssociations(fundingRuleId);
            var pointInTime = DateTime.UtcNow.AddDays(Random.Shared.Next(-50, 50));
            var sqlFundingRule = new FundingRuleEntity
            {
                Id = fundingRuleId,
                RuleName = $"{nameof(CourseAgeCheckActivity.CourseAgeCheck)}_{index+1}",
                EffectiveFrom = pointInTime.AddDays(-Random.Shared.Next(1, 10)).Date,
                EffectiveTo = pointInTime.AddDays(Random.Shared.Next(1, 10)).Date,
                Parameters = JsonSerializer.Serialize(new CourseAgeCheckParameters { MinimumAge = 0, MaximumAge = 24 }),
                CourseAssociations = sqlCourseAssociations
            };
            _dbContext.FundingRules.Add(sqlFundingRule);
            
            var tsFundingRule = new FundingRuleTableEntity
            {
                PartitionKey = "LOCAL",
                RowKey = sqlFundingRule.Id.ToString(),
                RuleName = sqlFundingRule.RuleName,
                EffectiveFrom = sqlFundingRule.EffectiveFrom,
                EffectiveTo = sqlFundingRule.EffectiveTo,
                Parameters = sqlFundingRule.Parameters,
                Courses = JsonSerializer.Serialize(sqlCourseAssociations.Select(x => x.CourseId))
            };
            await fundingRuleClient.AddEntityAsync(tsFundingRule);
        }
        
        await _dbContext.SaveChangesAsync();
    }
    
    public static async Task DeleteAllTableStorageRowsAsync(TableClient tableClient)
    {
        var queryResults = tableClient.QueryAsync<TableEntity>(select: ["PartitionKey", "RowKey"]);
        List<TableTransactionAction> batchActions = [];

        await foreach (TableEntity entity in queryResults)
        {
            batchActions.Add(new TableTransactionAction(TableTransactionActionType.Delete, entity));
            if (batchActions.Count != 100) continue;
            await tableClient.SubmitTransactionAsync(batchActions);
            batchActions.Clear();
        }

        if (batchActions.Count > 0)
        {
            await tableClient.SubmitTransactionAsync(batchActions);
        }
    }
    
    private async Task WarmUpRepositories()
    {
        await _sqlRulesRepository.GetActiveRulesForDatesAsync(_dates);
        await _tableStorageRulesRepository.GetActiveRulesForDatesAsync(_dates);
    }
}