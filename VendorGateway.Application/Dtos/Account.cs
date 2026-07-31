namespace VendorGateway.Application.Dtos
{
    public sealed record CreateAccountRequest(string email);
    public sealed record UpdateAccountRequest(string email);

    public sealed record GetAccountVendorResponse(int id, string email);
    public sealed record UpdateAccountVendorResponse(int id);
    public sealed record CreateAccountVendorResponse(int id, string email);

}
