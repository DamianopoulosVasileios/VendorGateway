namespace VendorGateway.Application.Dtos
{
    public sealed record GetProductsResponse(
        int id,
        string Title,
        float Price,
        string Description,
        string Category,
        string Image
    );
}
