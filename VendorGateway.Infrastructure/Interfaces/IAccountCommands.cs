namespace VendorGateway.Infrastructure.Interfaces
{
    public interface IAccountCommands
    {
        Task CreateAsync(int id, string email, CancellationToken ct);
        Task DeleteAsync(int id, CancellationToken ct);
        Task UpdateAsync(Application.Entities.Account account, CancellationToken ct);
    }
}