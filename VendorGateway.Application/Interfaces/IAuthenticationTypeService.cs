namespace VendorGateway.Application.Interfaces
{
    public interface IAuthenticationTypeService
    {
        string GenerateToken(string userId);
    }
}
