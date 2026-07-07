using VendorGateway.Application.Dtos;

namespace VendorGateway.Application.Mappers
{
    public class ProductMappers
    {
        public static IEnumerable<Entities.Product> Map(IEnumerable<GetProductsResponse> productsFromStore)
        {
            return productsFromStore.Select(product => new Entities.Product
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
