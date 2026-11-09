using Microsoft.Extensions.DependencyInjection;
using SchoolAdmission.Business.Interfaces.Services;
using SchoolAdmission.Business.Services;

namespace SchoolAdmission.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(
        this IServiceCollection services)
    {
        services.AddScoped<
            IAdmissionClassService,
            AdmissionClassService>();

        services.AddScoped<
            IAdmissionHoldService,
            AdmissionHoldService>();

        return services;
    }
}