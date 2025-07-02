using ERP.SharedKernel.Utilities;

namespace ERP.Domain.ValueObjects
{
    public class Money : IEquatable<Money>
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }

        private Money() { } // EF Core

        public Money(decimal amount, string currency = "USD")
        {
            Guard.AgainstNegative(amount, nameof(amount));
            Guard.AgainstNullOrEmpty(currency, nameof(currency));

            Amount = amount;
            Currency = currency.ToUpperInvariant();
        }

        public static Money Zero(string currency = "USD") => new(0, currency);

        public Money Add(Money other)
        {
            Guard.Against(Currency != other.Currency, "Cannot add different currencies");
            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            Guard.Against(Currency != other.Currency, "Cannot subtract different currencies");
            Guard.Against(Amount < other.Amount, "Insufficient amount");
            return new Money(Amount - other.Amount, Currency);
        }

        public Money Multiply(decimal factor)
        {
            Guard.AgainstNegative(factor, nameof(factor));
            return new Money(Amount * factor, Currency);
        }

        public bool Equals(Money? other)
        {
            if (other is null) return false;
            return Amount == other.Amount && Currency == other.Currency;
        }

        public override bool Equals(object? obj) => Equals(obj as Money);
        public override int GetHashCode() => HashCode.Combine(Amount, Currency);
        public override string ToString() => $"{Amount:F2} {Currency}";

        public static bool operator ==(Money? left, Money? right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(Money? left, Money? right) => !(left == right);
    }
}
