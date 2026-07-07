namespace SFA.DAS.FundingRuleValidation.Jobs.Domain;

public record Course
{
    public required string Id { get ;set; }
    public int SequenceNumber { get; set; }
    public CourseType Type { get ;set; }
    public TrainingType TrainingType { get ;set; }
    public int? StandardCode { get; set; }
    public DateTime StartDate { get ;set; }
    public DateTime EndDate { get ;set; }
    public DateTime PlannedEndDate { get ;set; }
    public LearnerCourseStatus Status { get; set; }
    public int AgeAtStartOfCourse { get; set; }
}