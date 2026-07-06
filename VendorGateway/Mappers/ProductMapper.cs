using VendorGateway.Application.Dtos;
using VendorGateway.Contracts.Product.Responses;

namespace VendorGateway.Mappers
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
