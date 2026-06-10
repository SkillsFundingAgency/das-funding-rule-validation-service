using SFA.DAS.FundingRuleValidation.Jobs.Domain;

namespace SFA.DAS.FundingRuleValidation.Jobs.Data;

public class TableStorageRulesRepository: IRulesRepository
{
    public async Task<List<FundingRule>> GetAll()
    {
        return [];
    }
}