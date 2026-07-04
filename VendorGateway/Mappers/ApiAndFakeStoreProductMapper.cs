using VendorGateway.Contracts.Product.Responses;
using VendorGateway.Infrastructure.Contracts.Product.Responses;

namespace VendorGateway.Mappers
{
    public static class ApiAndFakeStoreProductMapper
    {
        public static ApiGetProductsResponse ToApi(FakeStoreGetProductsResponse response)
        {
            return new ApiGetProductsResponse(response.id, response.Title, response.Price, response.Description, response.Category, response.Image);
        }
        public static IEnumerable<ApiGetProductsResponse> ToApi(IEnumerable<FakeStoreGetProductsResponse> responses)
        {
            return responses.Select(ToApi);
        }
    }
}
