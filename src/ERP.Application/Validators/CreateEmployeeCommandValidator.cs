using ERP.Application.Commands.Employees;

using FluentValidation;

namespace ERP.Application.Validators
{
    public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
    {
        public CreateEmployeeCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Employee name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

            RuleFor(x => x.HireDate)
                .NotEmpty().WithMessage("Hire date is required")
                .LessThanOrEqualTo(DateTime.Today).WithMessage("Hire date cannot be in the future");

            RuleFor(x => x.Salary)
                .GreaterThan(0).When(x => x.Salary.HasValue)
                .WithMessage("Salary must be greater than 0");

            RuleFor(x => x.LeaveBalance)
                .GreaterThanOrEqualTo(0).WithMessage("Leave balance cannot be negative");

            RuleFor(x => x.Position)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Position))
                .WithMessage("Position cannot exceed 100 characters");
        }
    }
}