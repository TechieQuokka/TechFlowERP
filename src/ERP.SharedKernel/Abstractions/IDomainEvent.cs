namespace ERP.SharedKernel.Abstractions
{
    /// <summary>
    /// 도메인 이벤트 인터페이스
    /// Event-Driven Architecture 지원
    /// </summary>
    public interface IDomainEvent
    {
        Guid Id { get; }
        DateTime OccurredOn { get; }
        string EventType { get; }
    }
}
