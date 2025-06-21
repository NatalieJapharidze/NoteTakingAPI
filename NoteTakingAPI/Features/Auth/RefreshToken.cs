using System.Security.Claims;
using FluentValidation;
using NoteTakingAPI.Infrastructure.Services;

namespace NoteTakingAPI.Features.Auth
{
    public class RefreshToken
    {
        public record Command(string Token, string RefreshTokenValue);
        public record Response(string Token, string RefreshToken);

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Token).NotEmpty();
                RuleFor(x => x.RefreshTokenValue).NotEmpty();
            }
        }

        public class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPost("/auth/refresh", Handle)
                   .WithOpenApi()
                   .WithSummary("Refresh authentication token")
                   .WithDescription("Refreshes expired authentication token");

            static async Task<IResult> Handle(
                Command command,
                IJwtService jwtService,
                IValidator<Command> validator,
                ILogger<RefreshToken> logger,
                CancellationToken ct)
            {
                var validationResult = await validator.ValidateAsync(command, ct);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var principal = jwtService.GetPrincipalFromExpiredToken(command.Token);
                if (principal is null)
                {
                    return Results.Unauthorized();
                }

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                var emailClaim = principal.FindFirst(ClaimTypes.Email);

                if (userIdClaim is null || emailClaim is null)
                {
                    return Results.Unauthorized();
                }

                if (!int.TryParse(userIdClaim.Value, out var userId))
                {
                    logger.LogWarning("Invalid user ID in token: {UserId}", userIdClaim.Value);
                    return Results.Unauthorized();
                }

                var newToken = jwtService.GenerateToken(userId, emailClaim.Value);
                var newRefreshToken = jwtService.GenerateRefreshToken();

                logger.LogInformation("Token refreshed for user {UserId}", userId);

                var response = new Response(newToken, newRefreshToken);
                return Results.Ok(response);
            }
        }
    }
}
