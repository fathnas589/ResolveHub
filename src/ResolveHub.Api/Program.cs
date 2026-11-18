using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ResolveHub.Api.Infrastructure;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.DataAccess.Connection;
using ResolveHub.DataAccess.Repositories;
using ResolveHub.Services.Implementations;
using ResolveHub.Services.Interfaces;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
              {
                  if (string.IsNullOrEmpty(origin)) return false;
                  try
                  {
                      var host = new Uri(origin).Host;
                      return host == "localhost" || host == "127.0.0.1";
                  }
                  catch
                  {
                      return false;
                  }
              })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Timestamps are stored as UTC; make sure clients receive them with a 'Z' suffix.
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
    });

// Consistent JSON error responses for unhandled exceptions (e.g. RAISERROR from stored procedures)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// JWT Authentication Configuration
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ResolveHub";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ResolveHubClient";
// The signing key is a secret: it is NOT stored in appsettings.json. Provide it via
//   development:  dotnet user-secrets set "Jwt:Key" "<random 64+ char value>" --project src/ResolveHub.Api
//   production:   environment variable Jwt__Key (or a secret store such as Azure Key Vault)
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.StartsWith("CHANGE_ME", StringComparison.Ordinal) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key is not configured. Set a random key of at least 32 characters with " +
        "'dotnet user-secrets set \"Jwt:Key\" \"<value>\" --project src/ResolveHub.Api' (development) " +
        "or the 'Jwt__Key' environment variable (production).");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// OpenAPI documentation with JWT Bearer security scheme
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT Bearer token."
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = securityScheme;
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        });

        return Task.CompletedTask;
    });
});

// Register DataAccess dependencies
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IDatabaseHealthCheck, DatabaseHealthCheck>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ISlaPolicyRepository, SlaPolicyRepository>();
builder.Services.AddScoped<ITicketSlaRepository, TicketSlaRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationDeliveryRepository, NotificationDeliveryRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

// Register Service & Security dependencies
builder.Services.AddSingleton<IPasswordHasher<string>, PasswordHasher<string>>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISubCategoryService, SubCategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ISlaPolicyService, SlaPolicyService>();

// AI triage (Claude). Optional: enabled only when an API key is configured via
//   dotnet user-secrets set "Ai:ApiKey" "<key>" --project src/ResolveHub.Api   (or the ANTHROPIC_API_KEY env var)
var aiApiKey = builder.Configuration["Ai:ApiKey"];
var aiModel = builder.Configuration["Ai:Model"] ?? "claude-opus-5-5";
Anthropic.AnthropicClient? anthropicClient =
    !string.IsNullOrWhiteSpace(aiApiKey) ? new Anthropic.AnthropicClient { ApiKey = aiApiKey } :
    !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")) ? new Anthropic.AnthropicClient() :
    null;
builder.Services.AddSingleton(new AiClientHolder(anthropicClient, aiModel));
builder.Services.AddScoped<IAiTriageService, AiTriageService>();

// Register Notification & Email options and services
builder.Services.Configure<ResolveHub.Services.Email.EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<ResolveHub.Services.Notifications.NotificationDeliveryOptions>(builder.Configuration.GetSection("NotificationDelivery"));
builder.Services.AddTransient<ResolveHub.Services.Email.IEmailSender, ResolveHub.Services.Email.SmtpEmailSender>();
builder.Services.AddScoped<ResolveHub.Services.Notifications.INotificationService, ResolveHub.Services.Notifications.NotificationService>();
builder.Services.AddScoped<ResolveHub.Services.Notifications.INotificationDeliveryService, ResolveHub.Services.Notifications.NotificationDeliveryService>();

// Register Background Services
builder.Services.AddHostedService<ResolveHub.Services.Workers.SlaMonitoringWorker>();
builder.Services.AddHostedService<ResolveHub.Services.Workers.NotificationDeliveryWorker>();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Swagger UI at /swagger
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "ResolveHub API v1");
        options.RoutePrefix = "swagger";
    });

    // Modern Scalar API reference UI at /scalar/v1
    app.MapScalarApiReference();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
