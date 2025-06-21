using NoteTakingAPI.Infrastructure.Services;
using static NoteTakingAPI.Infrastructure.Database.Entities.AppDbContext;
using System.Text;
using FluentValidation;
using System.Security.Cryptography;
using NoteTakingAPI.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace NoteTakingAPI.Features.Auth
{
    public class Register
    {
        public record Command(string Email, string Password, string FullName);
        public record Response(int Id, string Email, string FullName, string Token);

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
                RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            }
        }

        public class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("/auth/register", Handle)
                   .WithOpenApi()
                   .WithSummary("Register a new user")
                   .WithDescription("Creates a new user account and returns authentication token");

            static async Task<IResult> Handle(
                Command command,
                AppDbContext db,
                IJwtService jwtService,
                IValidator<Command> validator,
                ILogger<Register> logger,
                CancellationToken ct)
            {
                var validationResult = await validator.ValidateAsync(command, ct);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var existingUser = await db.Users
                    .FirstOrDefaultAsync(u => u.Email == command.Email, ct);

                if (existingUser is not null)
                {
                    return Results.Conflict(new { message = "User with this email already exists" });
                }

                var passwordHash = HashPassword(command.Password);

                var user = new User
                {
                    Email = command.Email,
                    PasswordHash = passwordHash,
                    FullName = command.FullName
                };

                db.Users.Add(user);
                await db.SaveChangesAsync(ct);

                var token = jwtService.GenerateToken(user.Id, user.Email);

                logger.LogInformation("User {Email} registered successfully", command.Email);

                var response = new Response(user.Id, user.Email, user.FullName, token);
                return Results.Created($"/users/{user.Id}", response);
            }

            private static string HashPassword(string password)
            {
                using var sha256 = SHA256.Create();
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
