namespace VendorGateway.Application.Dtos
{
    public sealed record ProductResponse(
        int Id,
        string Title,
        float Price,
        string Description,
        string Category,
        string Image,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);
}
