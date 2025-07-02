using ERP.Application.Commands.Projects;

using FluentValidation;

namespace ERP.Application.Validators
{
    public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
    {
        public CreateProjectCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Project name is required")
                .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters");

            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("Client is required");

            RuleFor(x => x.ManagerId)
                .NotEmpty().WithMessage("Manager is required");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required")
                .GreaterThanOrEqualTo(DateTime.Today.AddDays(-30))
                .WithMessage("Start date cannot be more than 30 days in the past");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .When(x => x.EndDate.HasValue)
                .WithMessage("End date must be after start date");

            RuleFor(x => x.Budget)
                .GreaterThan(0).WithMessage("Budget must be greater than 0");

            RuleFor(x => x.ProfitMargin)
                .InclusiveBetween(0, 100).WithMessage("Profit margin must be between 0 and 100");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .Length(3).WithMessage("Currency must be 3 characters (e.g., USD)");

            RuleFor(x => x.CodePrefix)
                .NotEmpty().WithMessage("Code prefix is required")
                .Matches("^[A-Z]{2,4}$").WithMessage("Code prefix must be 2-4 uppercase letters");
        }
    }
}
