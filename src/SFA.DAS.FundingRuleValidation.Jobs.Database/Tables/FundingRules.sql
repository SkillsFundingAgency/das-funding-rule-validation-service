CREATE TABLE dbo.[FundingRules] (
    [Id]                            UNIQUEIDENTIFIER        NOT NULL,
    [RuleName]                      NVARCHAR(255)           NOT NULL,
    [EffectiveFrom]                 DATETIMEOFFSET          NOT NULL,
    [EffectiveTo]                   DATETIMEOFFSET          NOT NULL,
    [Parameters]                    NVARCHAR(MAX)           NULL,
    CONSTRAINT [PK_FundingRules] PRIMARY KEY (Id),
    INDEX [IX_EffectiveDates] NONCLUSTERED([EffectiveFrom], [EffectiveTo]) INCLUDE ([Id], [RuleName], [Parameters]),
)