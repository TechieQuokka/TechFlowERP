namespace ERP.Application.DTOs
{
    public class ClientDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = default!;
        public string? Industry { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
        public decimal? ContractValue { get; set; }
        public string? ClientSize { get; set; }
        public int TotalProjectsCount { get; set; }
        public int ActiveProjectsCount { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageProjectBudget { get; set; }
        public bool IsVipClient { get; set; }
        public bool IsNewClient { get; set; }
        public string ClientImportance { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}