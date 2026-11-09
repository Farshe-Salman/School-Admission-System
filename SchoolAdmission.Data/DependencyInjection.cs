using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolAdmission.Business.Interfaces;
using SchoolAdmission.Business.Interfaces.Repositories;
using SchoolAdmission.Data.Persistence;
using SchoolAdmission.Data.Repositories;

namespace SchoolAdmission.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register PostgreSQL DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));

        // Register Generic Repository
        services.AddScoped(
            typeof(IGenericRepository<>),
            typeof(GenericRepository<>));

        // Register Specific Repositories
        services.AddScoped<
            IAdmissionClassRepository,
            AdmissionClassRepository>();

        services.AddScoped<
            IAdmissionHoldRepository,
            AdmissionHoldRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}