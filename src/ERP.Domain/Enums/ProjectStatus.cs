namespace ERP.Domain.Enums
{
    public enum ProjectStatus
    {
        Planning = 1,
        Active = 2,
        OnHold = 3,
        Completed = 4,
        Cancelled = 5
    }

    public enum ProjectType
    {
        FixedPrice = 1,
        TimeAndMaterial = 2,
        Retainer = 3
    }

    public enum RiskLevel
    {
        Low = 1,
        Medium = 2,
        High = 3
    }

    public enum EmployeeStatus
    {
        Active = 1,
        Inactive = 2,
        OnLeave = 3,
        Terminated = 4
    }

    public enum SkillLevel
    {
        Beginner = 1,
        Intermediate = 2,
        Advanced = 3,
        Expert = 4
    }

    public enum InvoiceStatus
    {
        Draft = 1,
        Sent = 2,
        Paid = 3,
        Overdue = 4,
        Cancelled = 5
    }

    public enum MilestoneStatus
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3,
        Delayed = 4
    }
}