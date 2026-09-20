using VendorGateway.Application.Common;

namespace VendorGateway.Application.Interfaces.CommandsQueries
{
    public interface IAccountCommands
    {
        Task<Result> CreateAsync(int id, string email, CancellationToken ct);
        Task<Result> DeleteAsync(int id, CancellationToken ct);
        Task<Result> UpdateAsync(int id, string email, CancellationToken ct);
    }
}
