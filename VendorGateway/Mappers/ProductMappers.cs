using VendorGateway.Contracts.Product.Responses;

namespace VendorGateway.Mappers
{
    public static class ProductMappers
    {
        public static IEnumerable<Infrastructure.Entities.Product> Map(IEnumerable<ApiGetProductsResponse> productsFromStore)
        {
            return productsFromStore.Select(product => new Infrastructure.Entities.Product
            {
                Id = product.id,
                Title = product.Title,
                Price = product.Price,
                Description = product.Description,
                Category = product.Category,
                Image = product.Image
            });
        }
    }
}
