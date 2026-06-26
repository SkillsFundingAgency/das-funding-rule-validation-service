using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.FundingRuleValidation.Jobs.Core;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data.Sql;

public interface IFundingRulesDataContext
{
    DbSet<FundingRuleEntity> FundingRules { get; }
    DbSet<FundingRuleCourseAssociationsEntity> CourseAssociations { get; }
}

[ExcludeFromCodeCoverage]
public class FundingRulesDbContext(DbContextOptions<FundingRulesDbContext> options) : DbContext(options), IFundingRulesDataContext
{
    public DbSet<FundingRuleEntity> FundingRules { get; set; }
    public DbSet<FundingRuleCourseAssociationsEntity> CourseAssociations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var fundingRuleModelBuilder = modelBuilder.Entity<FundingRuleEntity>();

        fundingRuleModelBuilder
            .ToTable(GlobalConstants.FundingRulesTableName)
            .HasMany(x => x.CourseAssociations)
            .WithOne(x => x.FundingRule)
            .HasForeignKey(x => x.FundingRuleId);

        fundingRuleModelBuilder.HasKey(x => x.Id);
        
        var courseAssociationsModelBuilder = modelBuilder.Entity<FundingRuleCourseAssociationsEntity>();
        courseAssociationsModelBuilder
            .ToTable(GlobalConstants.FundingRuleCourseAssociationsTableName)
            .HasOne(x => x.FundingRule)
            .WithMany(x => x.CourseAssociations)
            .HasForeignKey(x => x.FundingRuleId);
        
        courseAssociationsModelBuilder.HasKey(x => new { x.FundingRuleId, x.CourseId });
        
    }
}