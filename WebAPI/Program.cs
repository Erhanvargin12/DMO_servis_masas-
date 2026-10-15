using Autofac;
using Autofac.Extensions.DependencyInjection;
using Business.DependencyResolvers.Autofac;
using Core.DependencyResolvers;
using Core.Extensions;
using Core.Utilities.IoC;

using WebAPI.Jobs;
using WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Özel Serilog yapılandırmamız
builder.AddCustomSerilog();

// Servisleri ekle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddHostedService<TicketAutoCloseJob>();

// HttpClient Registration
builder.Services.AddHttpClient("gateway", client =>
{
    var gatewayUrl = builder.Configuration["GatewayApiBaseUrl"];
    if (!string.IsNullOrEmpty(gatewayUrl))
    {
        client.BaseAddress = new Uri(gatewayUrl);
    }
});
builder.Services.AddHttpClient("KIP", client =>
{
    var gatewayUrl = builder.Configuration["KIPApiBaseUrl"];
    if (!string.IsNullOrEmpty(gatewayUrl))
    {
        client.BaseAddress = new Uri(gatewayUrl);
    }
});
builder.Services.AddHttpClient("GeminiClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});


// Autofac ile bağımlılık enjeksiyonu yapılabilmesi için gerekli ayarlar
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder=>{containerBuilder.RegisterModule(new AutofacBusinessModule());});

builder.Services.AddCustomDatabase(builder.Configuration);

// JWT ayarları
builder.Services.AddCustomAuthentication(builder.Configuration);

builder.Services.AddDependencyResolvers(new ICoreModule[] {  new CoreModule() });

var app = builder.Build();

app.UseMiddleware<HttpTrafficLoggingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.ConfigureCustomExceptionMiddleware();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
