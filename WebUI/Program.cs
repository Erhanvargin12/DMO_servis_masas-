using Core.Communication;
using Microsoft.Extensions.FileProviders;
using WebUI.Core.Middleware;
using WebUI.Core.Services.Abstract;
using WebUI.Core.Services.Concrete;
using WebUI.Models.ViewModels;
using WebUI.Services.Abstract;
using WebUI.Services.Concrete;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("SessionSettings:IdleTimeout", 30));
    options.Cookie.Name = builder.Configuration.GetValue<string>("SessionSettings:CookieName", ".BackendCore.Session");
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddAuthentication("Cookies").AddCookie("Cookies", options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.Redirect("/Account/Login");
        return Task.CompletedTask;
    };
});

builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.DefaultRequestHeaders.ConnectionClose = false; // Keep-Alive
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // Geliştirme ortamında localhost self-signed sertifika için SSL doğrulamasını atla
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});
builder.Services.AddHttpClient("KIP", client =>
{
    var gatewayUrl = builder.Configuration["KIPApiBaseUrl"];
    if (!string.IsNullOrEmpty(gatewayUrl))
    {
        client.BaseAddress = new Uri(gatewayUrl);
    }
});

// TicketController uses serviceId = "gateway" via IServiceCommunication.
// Keep an alias client name to avoid missing BaseAddress errors.
builder.Services.AddHttpClient("gateway", client =>
{
    var gatewayUrl = builder.Configuration["GatewayApiBaseUrl"];
    if (!string.IsNullOrEmpty(gatewayUrl))
    {
        client.BaseAddress = new Uri(gatewayUrl);
    }
});


builder.Services.AddScoped<IServiceCommunication, HttpServiceCommunication>();
builder.Services.AddHttpClient("ChatbotClient");
builder.Services.AddScoped<IChatbotApiService, ChatbotApiService>();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation().AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = null; });
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();





var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Areas")),
    RequestPath = "/area-assets"
});

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseTokenRefresh();
app.UseJwtUserClaims();
app.UseAuthorization();
app.UseMenuAccess();

app.MapControllerRoute(name: "talepler_legacy1", pattern: "Talepler/Talep/{action=Index}/{id?}", defaults: new { area = "Tickets", controller = "Ticket" });
app.MapControllerRoute(name: "talepler_legacy2", pattern: "Talepler/Ticket/{action=Index}/{id?}", defaults: new { area = "Tickets", controller = "Ticket" });
app.MapControllerRoute(name: "talep_legacy", pattern: "Talep/{action=Index}/{id?}", defaults: new { area = "Tickets", controller = "Ticket" });
app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
