using VendorGateway.Application.Common;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Product
{
    public class DeleteProductService(IProductCommands productCommands) : IDeleteProductService
    {
        public async Task<Result> DeleteAsync(CancellationToken ct)
        {
            await productCommands.DeleteAsync(ct);
            return Result.Success();
        }
    }
}
