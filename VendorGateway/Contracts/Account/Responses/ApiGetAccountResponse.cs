namespace VendorGateway.API.Contracts.Account.Responses
{
    public sealed record ApiGetAccountResponse(int Id, string Email, IReadOnlyCollection<Application.Entities.Order> Orders, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
}
