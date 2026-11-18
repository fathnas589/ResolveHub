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

// AI triage (optional). Pick a provider with Ai:Provider = "Gemini" (free tier) or "Claude".
//   Gemini:  dotnet user-secrets set "Ai:Gemini:ApiKey" "<key>" --project src/ResolveHub.Api   (or GEMINI_API_KEY)
//   Claude:  dotnet user-secrets set "Ai:Claude:ApiKey" "<key>" --project src/ResolveHub.Api   (or ANTHROPIC_API_KEY)
// With no key for the selected provider, AI features are hidden.
var aiProvider = builder.Configuration["Ai:Provider"] ?? "Gemini";
builder.Services.AddHttpClient("gemini", c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    ResolveHub.Services.Ai.ITriageModelClient? client = null;

    if (aiProvider.Equals("Claude", StringComparison.OrdinalIgnoreCase))
    {
        var key = config["Ai:Claude:ApiKey"];
        var model = config["Ai:Claude:Model"] ?? "claude-opus-5-5";
        if (!string.IsNullOrWhiteSpace(key))
            client = new ResolveHub.Services.Ai.ClaudeTriageClient(new Anthropic.AnthropicClient { ApiKey = key }, model);
        else if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")))
            client = new ResolveHub.Services.Ai.ClaudeTriageClient(new Anthropic.AnthropicClient(), model);
    }
    else
    {
        var key = config["Ai:Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(key)) key = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        var model = config["Ai:Gemini:Model"] ?? "gemini-3.5-flash-lite";
        if (!string.IsNullOrWhiteSpace(key))
            client = new ResolveHub.Services.Ai.GeminiTriageClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("gemini"), key, model);
    }

    return new AiModelHolder(client);
});
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
