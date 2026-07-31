namespace VendorGateway.Application.Interfaces.CommandsQueries
{
    public interface IAccountCommands
    {
        Task CreateAsync(int id, string email, CancellationToken ct);
        Task DeleteAsync(int id, CancellationToken ct);
        Task UpdateAsync(int id, string email, CancellationToken ct);
    }
}