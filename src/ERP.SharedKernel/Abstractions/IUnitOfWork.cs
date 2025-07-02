namespace ERP.SharedKernel.Abstractions
{
    /// <summary>
    /// Unit of Work 패턴
    /// 트랜잭션 관리와 도메인 이벤트 발행
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}