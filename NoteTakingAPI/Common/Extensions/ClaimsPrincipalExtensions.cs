using System.Security.Claims;

namespace NoteTakingAPI.Common.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException("User is not authenticated");

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new InvalidOperationException("User ID claim is missing");

            if (!int.TryParse(userIdClaim, out var userId))
                throw new InvalidOperationException($"Invalid user ID format: {userIdClaim}");

            return userId;
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException("User is not authenticated");

            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                throw new InvalidOperationException("Email claim is missing");

            return email;
        }
    }
}
