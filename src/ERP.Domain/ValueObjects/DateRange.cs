using ERP.SharedKernel.Utilities;

namespace ERP.Domain.ValueObjects
{
    public class DateRange : IEquatable<DateRange>
    {
        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }

        private DateRange() { } // EF Core

        public DateRange(DateTime startDate, DateTime? endDate = null)
        {
            Guard.Against(endDate.HasValue && endDate <= startDate,
                "End date must be after start date");

            StartDate = startDate.Date;
            EndDate = endDate?.Date;
        }

        public bool IsActive => !EndDate.HasValue || DateTime.Today <= EndDate.Value;
        public bool IsCompleted => EndDate.HasValue && DateTime.Today > EndDate.Value;
        public int DurationInDays => EndDate.HasValue
            ? (EndDate.Value - StartDate).Days + 1
            : (DateTime.Today - StartDate).Days + 1;

        public bool Contains(DateTime date)
        {
            var dateOnly = date.Date;
            return dateOnly >= StartDate && (EndDate == null || dateOnly <= EndDate);
        }

        public bool Overlaps(DateRange other)
        {
            Guard.AgainstNull(other, nameof(other));

            if (!EndDate.HasValue || !other.EndDate.HasValue)
                return true; // Open-ended ranges always overlap

            return StartDate <= other.EndDate && other.StartDate <= EndDate;
        }

        public bool Equals(DateRange? other)
        {
            if (other is null) return false;
            return StartDate == other.StartDate && EndDate == other.EndDate;
        }

        public override bool Equals(object? obj) => Equals(obj as DateRange);
        public override int GetHashCode() => HashCode.Combine(StartDate, EndDate);
        public override string ToString() =>
            $"{StartDate:yyyy-MM-dd} - {(EndDate?.ToString("yyyy-MM-dd") ?? "Ongoing")}";
    }
}
