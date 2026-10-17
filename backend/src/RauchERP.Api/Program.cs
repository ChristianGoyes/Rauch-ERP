using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RauchERP.Application.Abstractions;
using RauchERP.Infrastructure.Shared;
using RauchERP.Modules.Catalog.Infrastructure;
using RauchERP.Modules.Catalog.Presentation;
using RauchERP.Modules.Identity.Infrastructure;
using RauchERP.Modules.Identity.Presentation;
using RauchERP.Modules.Inventory.Infrastructure;
using RauchERP.Modules.Inventory.Presentation;
using RauchERP.Modules.Organization.Infrastructure;
using RauchERP.Modules.Organization.Presentation;
using RauchERP.Modules.Sales.Infrastructure;
using RauchERP.Modules.Sales.Presentation;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Host.UseSerilog((context, log) => log.ReadFrom.Configuration(context.Configuration).WriteTo.Console());
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<HttpExecutionContext>();
builder.Services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<HttpExecutionContext>());
builder.Services.AddScoped<ICompanyContext>(sp => sp.GetRequiredService<HttpExecutionContext>());
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"]).AllowAnyHeader().AllowAnyMethod()));
var signingKey = builder.Configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is required.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new() { ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"], IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)), ClockSkew = TimeSpan.FromSeconds(30) };
});
builder.Services.AddAuthorization(options => { foreach (var permission in RauchERP.Modules.Identity.Contracts.Permissions.All) options.AddPolicy(permission, policy => policy.RequireClaim("permission", permission)); });
builder.Services.AddIdentityModule(builder.Configuration).AddOrganizationModule(builder.Configuration).AddCatalogModule(builder.Configuration).AddInventoryModule(builder.Configuration).AddSalesModule(builder.Configuration);

var app = builder.Build();
app.UseExceptionHandler();
app.Use(async (context, next) => { const string name = "X-Correlation-ID"; var id = context.Request.Headers[name].FirstOrDefault() ?? Guid.NewGuid().ToString("N"); context.Response.Headers[name] = id; using (Serilog.Context.LogContext.PushProperty("CorrelationId", id)) await next(); });
app.UseSerilogRequestLogging();
app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new { service = "Rauch ERP API", version = "v1" })).AllowAnonymous();
app.MapIdentityEndpoints().MapOrganizationEndpoints().MapCatalogEndpoints().MapInventoryEndpoints().MapSalesEndpoints();

if (app.Environment.IsDevelopment() && app.Configuration.GetValue("Seed:Enabled", true))
{
    await DevelopmentDatabase.InitializeAsync(app.Services, app.Configuration);
}

await app.RunAsync();

public partial class Program;
