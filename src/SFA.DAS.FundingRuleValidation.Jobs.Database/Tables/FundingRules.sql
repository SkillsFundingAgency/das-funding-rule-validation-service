CREATE TABLE dbo.[FundingRules] (
    [Id]                            UNIQUEIDENTIFIER        NOT NULL,
    [RuleName]                      NVARCHAR(50)            NOT NULL,
    CONSTRAINT [PK_FundingRules] PRIMARY KEY (Id)
)