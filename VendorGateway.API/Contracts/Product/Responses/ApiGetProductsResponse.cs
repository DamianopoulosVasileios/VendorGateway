namespace VendorGateway.API.Contracts.Product.Responses
{
    public sealed record ApiGetProductsResponse(
        int id,
        string Title,
        float Price,
        string Description,
        string Category,
        string Image
    );
}
