using Microsoft.Extensions.DependencyInjection;
using VendorGateway.Application.Interfaces.Services;
using VendorGateway.Application.Services.Account;
using VendorGateway.Application.Services.Order;
using VendorGateway.Application.Services.Product;

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

            services.AddScoped<ICreateOrderService, CreateOrderService>();
            services.AddScoped<IDeleteOrderService, DeleteOrderService>();
            services.AddScoped<IGetOrderService, GetOrderService>();
            services.AddScoped<IUpdateOrderService, UpdateOrderService>();
            services.AddScoped<IExecuteOrderService, ExecuteOrderService>();

            services.AddScoped<ICreateProductService, CreateProductService>();
            services.AddScoped<IGetProductService, GetProductService>();
            services.AddScoped<IDeleteProductService, DeleteProductService>();
            return services;
        }
    }
}
