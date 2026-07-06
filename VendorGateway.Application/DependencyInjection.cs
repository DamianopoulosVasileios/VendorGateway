using Microsoft.Extensions.DependencyInjection;
using VendorGateway.Application.Interfaces.Services;
using VendorGateway.Application.Services.Account;

namespace VendorGateway.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServicesFromApplication(this IServiceCollection services)
        {
            services.AddScoped<ICreateAccountService, CreateAccountService>();
            services.AddScoped<IDeleteAccountService, DeleteAccountService>();
            services.AddScoped<IGetAccountService, GetAccountService>();
            services.AddScoped<IUpdateAccountService, UpdateAccountService>();
            return services;
        }
    }
}
