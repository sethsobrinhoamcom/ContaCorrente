using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ContaCorrente.Tarifas.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configuração
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Services
builder.Services.AddHostedService<TarifaConsumerService>();

var host = builder.Build();

Console.WriteLine("===========================================");
Console.WriteLine("   Sistema de Tarifas - BankMore");
Console.WriteLine("===========================================");
Console.WriteLine($"Iniciado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
Console.WriteLine("Aguardando transferências...");
Console.WriteLine("Pressione Ctrl+C para parar");
Console.WriteLine("===========================================\n");

await host.RunAsync();