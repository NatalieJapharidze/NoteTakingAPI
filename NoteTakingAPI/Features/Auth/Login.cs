using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NoteTakingAPI.Infrastructure.Database;
using NoteTakingAPI.Infrastructure.Services;
using System.Security.Cryptography;
using System.Text;

namespace NoteTakingAPI.Features.Auth
{
    public class Login
    {
        public record Command(string Email, string Password);
        public record Response(int Id, string Email, string FullName, string Token, string RefreshToken);

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.Password).NotEmpty();
            }
        }

        public class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("/auth/login", Handle)
                   .WithOpenApi()
                   .WithSummary("Login user")
                   .WithDescription("Authenticates user and returns authentication tokens");

            static async Task<IResult> Handle(
                Command command,
                AppDbContext db,
                IJwtService jwtService,
                IValidator<Command> validator,
                ILogger<Login> logger,
                CancellationToken ct)
            {
                var validationResult = await validator.ValidateAsync(command, ct);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var user = await db.Users
                    .FirstOrDefaultAsync(u => u.Email == command.Email, ct);

                if (user is null || !VerifyPassword(command.Password, user.PasswordHash))
                {
                    logger.LogWarning("Failed login attempt for email {Email}", command.Email);
                    return Results.Unauthorized();
                }

                var token = jwtService.GenerateToken(user.Id, user.Email);
                var refreshToken = jwtService.GenerateRefreshToken();

                logger.LogInformation("User {Email} logged in successfully", command.Email);

                var response = new Response(user.Id, user.Email, user.FullName, token, refreshToken);
                return Results.Ok(response);
            }

            private static bool VerifyPassword(string password, string hash)
            {
                using var sha256 = SHA256.Create();
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hashedPassword = Convert.ToBase64String(hashedBytes);
                return hashedPassword == hash;
            }
        }
    }
}
