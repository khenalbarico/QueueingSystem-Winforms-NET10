using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using LogicLibrary1;
using LogicLibrary1.AdmCntlrHandler1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace QueueingSystem1;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var builder = Host.CreateApplicationBuilder();

        builder.Services.RegisterServices();

        builder.Services.AddTransient<LoginForm1>();
        builder.Services.AddTransient<CreateAccountForm1>();
        builder.Services.AddTransient<DashboardForm1>();
        builder.Services.AddTransient<QueueModalForm1>();
        builder.Services.AddTransient<ManageQueuesControl1>();
        builder.Services.AddTransient<QueueController1>();
        builder.Services.AddTransient<ServicesControl1>();

        using var host = builder.Build();

        Application.Run(host.Services.GetRequiredService<LoginForm1>());
    }
}