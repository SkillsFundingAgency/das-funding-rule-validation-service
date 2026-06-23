using Azure.Data.Tables;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SFA.DAS.FundingRuleValidation.Jobs.Activities;
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
    
    private DateTime _date;

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

        _date = DateTime.UtcNow.Date;

        await InitialiseData();
        await WarmUpRepositories();
    }

    [BenchmarkCategory("GetActiveRulesForDate")]
    [Benchmark(Baseline = true)]
    public async Task<List<FundingRule>> SqlServer()
    {
        return await _sqlRulesRepository.GetActiveRulesForDate(_date);
    }

    [BenchmarkCategory("GetActiveRulesForDate")]
    [Benchmark]
    public async Task<List<FundingRule>> AzureTableStorage()
    {
        return await _tableStorageRulesRepository.GetActiveRulesForDate(_date);
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await _dbContext.DisposeAsync();
    }

    private async Task InitialiseData()
    {
        // SQL Server
        await _dbContext.FundingRules.ExecuteDeleteAsync();
        var sqlEntity = new FundingRuleEntity
        {
            Id = Guid.NewGuid(),
            RuleName = nameof(CourseAgeCheckActivity.CourseAgeCheck)
        };
        _dbContext.FundingRules.Add(sqlEntity);
        await _dbContext.SaveChangesAsync();
        
        // Table storage
        var client = _tableServiceClient.GetTableClient(GlobalConstants.FundingRulesTableName);
        await client.CreateIfNotExistsAsync();
        await DeleteAllTableStorageRowsAsync(client);
        var tsEntity = new FundingRuleTableEntity
        {
            PartitionKey = "LOCAL",
            RowKey = sqlEntity.Id.ToString(),
            RuleName = sqlEntity.RuleName
        };
        await client.AddEntityAsync(tsEntity);
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
        await _sqlRulesRepository.GetActiveRulesForDate(_date);
        await _tableStorageRulesRepository.GetActiveRulesForDate(_date);
    }
}