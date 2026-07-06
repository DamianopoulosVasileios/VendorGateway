namespace VendorGateway.Contracts.Product.Responses
{
    public sealed record FakeStoreGetProductsResponse(
        int id,
        string Title,
        float Price,
        string Description,
        string Category,
        string Image
    );
}
