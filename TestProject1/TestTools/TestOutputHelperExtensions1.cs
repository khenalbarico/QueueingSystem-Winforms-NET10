using LogicLibrary1.AdmCntlrHandler1;
using LogicLibrary1.AuthHandler1;
using LogicLibrary1.AuthHandler1.Interfaces;
using LogicLibrary1.QueueingHandler1;
using LogicLibrary1.QueueingHandler1.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

namespace TestProject1.TestTools;

internal static class TestOutputHelperExtensions1
{
    private static IHost? host;
    public static T Get<T>(this ITestOutputHelper ctx, Action<IServiceCollection>? svcModifier = null) where T : class
    {
        host ??= new HostBuilder().ConfigureServices(svc =>
        {
            svc.AddScoped<Authentication1>();
            svc.AddScoped<Queue1>();
            svc.AddScoped<QueueController1>();
            svc.AddScoped<ServicesController1>();
            svc.AddSingleton<IQueueIdGenerator1, QueueIdGenerator1>();
            svc.AddSingleton<ICurrentUser1, CurrentUser1>();
        })
        .Build();

        return host.Services.GetRequiredService<T>();
    }
}
