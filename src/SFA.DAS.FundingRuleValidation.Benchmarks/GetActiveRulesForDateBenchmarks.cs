using Azure.Data.Tables;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        var tableServiceClient = new TableServiceClient(tableStorageConnectionString);

        _sqlRulesRepository = new SqlRulesRepository(_dbContext);
        _tableStorageRulesRepository = new TableStorageRulesRepository(tableServiceClient);

        _date = DateTime.UtcNow.Date;

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

    private async Task WarmUpRepositories()
    {
        await _sqlRulesRepository.GetActiveRulesForDate(_date);
        await _tableStorageRulesRepository.GetActiveRulesForDate(_date);
    }
}