using VendorGateway.Application.Dtos;
using VendorGateway.Infrastructure.Apis.Contracts.Responses;

namespace VendorGateway.Infrastructure.Mappers
{
    public static class ProductMapper
    {
        public static GetProductsResponse ToApi(FakeStoreGetProductsResponse response)
        {
            return new GetProductsResponse(response.id, response.Title, response.Price, response.Description, response.Category, response.Image);
        }
        public static IEnumerable<GetProductsResponse> ToApi(IEnumerable<FakeStoreGetProductsResponse> responses)
        {
            return responses.Select(ToApi);
        }
    }
}
