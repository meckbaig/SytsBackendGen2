using FluentValidation;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Configuration;
using SytsBackendGen2.Application.Common.BaseRequests;
using SytsBackendGen2.Application.Common.Behaviours;
using SytsBackendGen2.Application.Common.Interfaces;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IRequestExceptionHandler<,,>), typeof(GlobalRequestExceptionHandler<,,>));

        return services;
    }
}
