using LogicLibrary1.AdmCntlrHandler1;
using LogicLibrary1.AuthHandler1;
using LogicLibrary1.AuthHandler1.Interfaces;
using LogicLibrary1.QueueingHandler1;
using LogicLibrary1.QueueingHandler1.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LogicLibrary1;

public static class ServiceRegistry
{
    public static void RegisterServices(this IServiceCollection svc)
    {
        svc.AddScoped<Authentication1>();
        svc.AddScoped<Queue1>();
        svc.AddScoped<QueueController1>();
        svc.AddScoped<ServicesController1>();
        svc.AddSingleton<IQueueIdGenerator1, QueueIdGenerator1>();
        svc.AddSingleton<ICurrentUser1, CurrentUser1>();
    }
}