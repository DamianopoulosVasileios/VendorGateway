using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Jobs.Commands;
using VendorGateway.Infrastructure.ExceptionClassifiers;
using VendorGateway.Infrastructure.Interfaces;
using VendorGateway.Infrastructure.Jobs.Commands;
using VendorGateway.Infrastructure.Persistence;
using VendorGateway.Infrastructure.Repositories.Account;
using VendorGateway.Infrastructure.Repositories.Authorization;
using VendorGateway.Infrastructure.Repositories.Order;
using VendorGateway.Infrastructure.Repositories.Product;

namespace VendorGateway.Infrastructure.Dependencies
{
    public static class DependencyInjection
    {
        public const string SqliteWriteResiliencePipeline = "sqlite-write";

        public static IServiceCollection AddServicesFromInfrastructure(this IServiceCollection services, string mode)
        {
            services.AddScoped<IAccountQueries, AccountQueries>();
            services.AddScoped<IAccountCommands, AccountCommands>();
            services.AddScoped<IProductQueries, ProductQueries>();
            services.AddScoped<IProductCommands, ProductCommands>();
            services.AddScoped<IOrderQueries, OrderQueries>();
            services.AddScoped<IOrderCommands, OrderCommands>();
            services.AddScoped<IAuthorizationQueries, AuthorizationQueries>();

            services.AddScoped<IJobCommands, JobCommands>();

            services.AddExceptionClassifierInfrastructure();

            services.AddResiliencePipeline(SqliteWriteResiliencePipeline, (builder, context) =>
            {
                var classifier = context.ServiceProvider.GetRequiredService<IDbExceptionClassifier>();
                builder.AddRetry(new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<DbUpdateException>(classifier.IsTransientBusyError),
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(50),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                });
            });

            var dbPath = GetPath(mode);

            services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite($"Data Source={dbPath}"));

            return services;
        }

        public static IServiceCollection AddExceptionClassifierInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IDbExceptionClassifier, SqliteExceptionClassifier>();
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
