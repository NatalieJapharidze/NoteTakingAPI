using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NoteTakingAPI.Features.Auth;
using NoteTakingAPI.Features.Notes;
using NoteTakingAPI.Features.Tags;
using NoteTakingAPI.Infrastructure.Database;
using NoteTakingAPI.Infrastructure.Middleware;
using NoteTakingAPI.Infrastructure.Services;
using Scalar.AspNetCore;
using Serilog;

namespace NoteTakingAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddControllers();

            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"]!;

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

            builder.Services.AddAuthorization();

            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            builder.Services.AddHealthChecks()
                .AddDbContextCheck<AppDbContext>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info = new()
                    {
                        Title = "Note Taking API",
                        Version = "v1",
                        Description = "A modern REST API for managing personal notes with JWT authentication",
                        Contact = new()
                        {
                            Name = "API Support",
                            Email = "support@noteapi.com"
                        }
                    };

                    document.Components ??= new();
                    document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
                    document.Components.SecuritySchemes["Bearer"] = new()
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Description = "Enter your JWT token to access protected endpoints"
                    };

                    return Task.CompletedTask;
                });
            });

            var app = builder.Build();

            app.UseMiddleware<ExceptionMiddleware>();

            app.MapHealthChecks("/health");

            app.UseAuthentication();
            app.UseAuthorization();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference(options =>
                {
                    options.Title = "Note Taking API";
                    options.Theme = ScalarTheme.BluePlanet;
                    options.ShowSidebar = true;
                    options.Authentication = new ScalarAuthenticationOptions
                    {
                        PreferredSecurityScheme = "Bearer"
                    };
                });
            }

            Register.Endpoint.Map(app);
            Login.Endpoint.Map(app);
            RefreshToken.Endpoint.Map(app);
            GetNotes.Endpoint.Map(app);
            GetNoteById.Endpoint.Map(app);
            CreateNote.Endpoint.Map(app);
            UpdateNote.Endpoint.Map(app);
            DeleteNote.Endpoint.Map(app);
            GetTags.Endpoint.Map(app);

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.Migrate();
            }

            app.Run();
        }

        static async Task EnsureDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Checking database connection...");

                if (await context.Database.CanConnectAsync())
                {
                    logger.LogInformation("Database connection successful");

                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("Applying {Count} pending migrations", pendingMigrations.Count());
                        await context.Database.MigrateAsync();
                        logger.LogInformation("Database migrations applied successfully");
                    }
                    else
                    {
                        logger.LogInformation("Database is up to date");
                    }
                }
                else
                {
                    logger.LogWarning("Cannot connect to database. Please ensure PostgreSQL is running and connection string is correct.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while ensuring database");
            }
        }
    }
}
