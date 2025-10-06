using System.Text;
using Api.Endpoints.Abstractions;
using Api.Middleware;
using Application;
using Application.Behaviors;
using Application.Common.Mapping;
using Cortex.Mediator;
using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using FluentValidation;
using Infrastructure;
using Infrastructure.Persistence;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Json;
using ProblemDetailsExtensions = Hellang.Middleware.ProblemDetails.ProblemDetailsExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(new JsonFormatter());
});

// Add minimal MVC services for ProblemDetails
builder.Services.AddControllers();

builder.Services.AddExceptionMapping(builder.Environment);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddValidatorsFromAssembly(AssemblyReference.Assembly);

// Register Cortex.Mediator
builder.Services.AddScoped<IMediator, Mediator>();

// Register handlers from Application assembly
foreach (var type in typeof(AssemblyReference).Assembly.GetTypes())
{
    var commandInterface = type.GetInterfaces().FirstOrDefault(i =>
        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>));
    if (commandInterface != null) builder.Services.AddScoped(commandInterface, type);

    var queryInterface = type.GetInterfaces().FirstOrDefault(i =>
        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));
    if (queryInterface != null) builder.Services.AddScoped(queryInterface, type);
}

// Register behaviors
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(TransactionBehavior<,>));
builder.Services.AddScoped(typeof(IQueryPipelineBehavior<,>), typeof(ValidationQueryBehavior<,>));
builder.Services.AddScoped(typeof(IQueryPipelineBehavior<,>), typeof(PerformanceQueryBehavior<,>));
builder.Services.AddScoped(typeof(IQueryPipelineBehavior<,>), typeof(LoggingQueryBehavior<,>));

MapsterConfig.Register(TypeAdapterConfig.GlobalSettings);
builder.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
                         Array.Empty<string>();
    options.AddPolicy("CorsPolicy", policy =>
    {
        if (allowedOrigins.Length == 0)
            policy.AllowAnyOrigin();
        else
            policy.WithOrigins(allowedOrigins).AllowCredentials();

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSection = builder.Configuration.GetSection("Jwt");
        var issuer = jwtSection.GetValue<string>("Issuer");
        var audience = jwtSection.GetValue<string>("Audience");
        var key = jwtSection.GetValue<string>("Key") ??
                  throw new InvalidOperationException("JWT key is not configured.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization(options => { options.AddPolicy("Admin", policy => policy.RequireRole("Admin")); });

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Clean Backend API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Input your JWT token as: {token}",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

var applyMigrations = builder.Configuration.GetValue<bool?>("Database:ApplyMigrationsOnStartup")
                      ?? app.Environment.IsDevelopment();

if (applyMigrations)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSerilogRequestLogging();
ProblemDetailsExtensions.UseProblemDetails(app);

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/healthz");
app.MapHealthChecks("/readyz");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapDiscoveredEndpoints(typeof(Program).Assembly);

await app.RunAsync();
