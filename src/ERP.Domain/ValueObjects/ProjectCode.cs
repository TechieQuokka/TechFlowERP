using System.Text.RegularExpressions;

using ERP.SharedKernel.Utilities;

namespace ERP.Domain.ValueObjects
{
    public class ProjectCode : IEquatable<ProjectCode>
    {
        private static readonly Regex CodePattern = new(@"^[A-Z]{2,4}-\d{4}-\d{2}$", RegexOptions.Compiled);

        public string Value { get; private set; }

        private ProjectCode() { } // EF Core

        public ProjectCode(string value)
        {
            Guard.AgainstNullOrEmpty(value, nameof(value));
            Guard.Against(!CodePattern.IsMatch(value),
                "Project code must follow pattern: XX-YYYY-MM (e.g., WEB-2024-01)");

            Value = value.ToUpperInvariant();
        }

        public static ProjectCode Generate(string prefix, DateTime date)
        {
            Guard.AgainstNullOrEmpty(prefix, nameof(prefix));
            Guard.Against(prefix.Length < 2 || prefix.Length > 4,
                "Prefix must be 2-4 characters");

            var year = date.Year;
            var month = date.Month;
            return new ProjectCode($"{prefix.ToUpper()}-{year}-{month:D2}");
        }

        public bool Equals(ProjectCode? other)
        {
            if (other is null) return false;
            return Value == other.Value;
        }

        public override bool Equals(object? obj) => Equals(obj as ProjectCode);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;

        public static implicit operator string(ProjectCode code) => code.Value;
        public static explicit operator ProjectCode(string value) => new(value);
    }
}
