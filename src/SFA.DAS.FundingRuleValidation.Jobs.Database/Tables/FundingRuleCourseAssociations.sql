CREATE TABLE dbo.[FundingRuleCourseAssociations] (
    [FundingRuleId]                 UNIQUEIDENTIFIER        NOT NULL,
    [CourseId]                      NVARCHAR(255)           NOT NULL,
    CONSTRAINT [PK_FundingRuleCourseAssociations] PRIMARY KEY (FundingRuleId, CourseId),
    CONSTRAINT [FK_FundingRuleCourseAssociations_FundingRules] FOREIGN KEY (FundingRuleId) REFERENCES [FundingRules](Id)
)