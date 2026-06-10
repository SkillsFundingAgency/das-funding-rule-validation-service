using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data.Sql;

public interface IFundingRulesDataContext
{
    DbSet<FundingRuleEntity> FundingRules { get; }
}

internal class FundingRulesDbContext(DbContextOptions<FundingRulesDbContext> options) : DbContext(options), IFundingRulesDataContext
{
    public DbSet<FundingRuleEntity> FundingRules { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var fundingRuleModelBuilder = modelBuilder.Entity<FundingRuleEntity>();

        fundingRuleModelBuilder.ToTable("FundingRules");
        fundingRuleModelBuilder.HasKey(x => x.Id);
        fundingRuleModelBuilder
            .Property(x => x.RuleName)
            .HasColumnName("RuleName")
            .HasColumnType("NVARCHAR(50)")
            .IsRequired();
    }
}