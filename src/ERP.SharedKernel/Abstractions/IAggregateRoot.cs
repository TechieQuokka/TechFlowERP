namespace ERP.SharedKernel.Abstractions
{
    /// <summary>
    /// 도메인 엔티티의 최상위 인터페이스
    /// DDD의 Aggregate Root 패턴 구현
    /// </summary>
    public interface IAggregateRoot
    {
        Guid Id { get; }
        string TenantId { get; }  // Multi-tenant 지원
        DateTime CreatedAt { get; }
        DateTime UpdatedAt { get; }

        // Domain Events 관리
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
        void ClearDomainEvents();
    }
}
