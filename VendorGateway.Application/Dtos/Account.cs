namespace VendorGateway.Application.Dtos
{
    public sealed record CreateAccountRequest(int id, string email);
    public sealed record UpdateAccountRequest();

    public sealed record GetAccountVendorResponse(int id, string email);
    public sealed record UpdateAccountVendorResponse(int id);
    public sealed record CreateAccountVendorResponse(int id, string email);

}
