using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VendorGateway.Infrastructure.ExceptionClassifiers;
using VendorGateway.Infrastructure.Interfaces;
using VendorGateway.Infrastructure.Persistence;
using VendorGateway.Infrastructure.Repositories.Account;
using VendorGateway.Infrastructure.Repositories.Order;
using VendorGateway.Infrastructure.Repositories.Product;

namespace VendorGateway.Infrastructure.Dependencies
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServicesFromInfrastructure(this IServiceCollection services, string mode, string connString)
        {
            services.AddScoped<IAccountQueries, AccountQueries>();
            services.AddScoped<IProductCommands, ProductCommands>();
            services.AddScoped<IOrderCommands, OrderCommands>();
            services.AddScoped<IOrderQueries, OrderQueries>();
            services.AddScoped<IAccountCommands, AccountCommands>();
            services.AddScoped<IProductQueries, ProductQueries>();

            services.AddExceptionClassifierInfrastructure();

            services.AddInfrastructure(connString);

            var dbPath = GetPath(mode);

            services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

            return services;
        }

        public static IServiceCollection AddExceptionClassifierInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IDbExceptionClassifier, SqlServerExceptionClassifier>();
            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connString)
        {
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connString));
            return services;
        }

        public static string GetPath(string envirnoment)
        {
            var projectFolder = "Infrastructure";
            var dbFolderName = "DbFile";

            var path = Path.Combine(projectFolder, dbFolderName);

            Directory.CreateDirectory(path);

            return Path.Combine(path, "VendorGateway.db");
        }
    }
}
