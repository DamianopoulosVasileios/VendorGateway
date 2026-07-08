using VendorGateway.Application.Dtos;

namespace VendorGateway.API.Mappers
{
    public static class ProductMappers
    {
        public static ProductResponse ToApiResponse(this Application.Entities.Product product)
        {
            return new ProductResponse(product.Id, product.Title, product.Price, product.Description, product.Category, product.Image, product.CreatedAt, product.UpdatedAt);
        }
    }
}
