using System.ComponentModel.DataAnnotations;

using ERP.SharedKernel.Utilities;

namespace ERP.SharedKernel.Abstractions
{
    /// <summary>
    /// 모든 엔티티의 기본 클래스
    /// </summary>
    public abstract class BaseEntity : IAggregateRoot
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        protected BaseEntity(string tenantId) : this()
        {
            Guard.AgainstNullOrEmpty(tenantId, nameof(tenantId));
            TenantId = tenantId;
        }

        [Key]
        public Guid Id { get; private set; }

        [Required]
        [MaxLength(36)]
        public string TenantId { get; private set; } = default!;

        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        protected void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not BaseEntity other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Id == other.Id && TenantId == other.TenantId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, TenantId);
        }
    }
}