using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NoteTakingAPI.Common.Extensions;
using NoteTakingAPI.Infrastructure.Database;

namespace NoteTakingAPI.Features.Tags
{
    public class GetTags
    {
        public record Response(List<TagItem> Tags);
        public record TagItem(int Id, string Name, int NotesCount);

        public class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapGet("/tags", Handle)
                   .RequireAuthorization()
                   .WithOpenApi()
                   .WithSummary("Get user tags")
                   .WithDescription("Retrieves all tags used by the user");

            static async Task<IResult> Handle(
                ClaimsPrincipal user,
                AppDbContext db,
                ILogger<GetTags> logger,
                CancellationToken ct)
            {
                var userId = user.GetUserId();

                var tagData = await db.Tags
                    .Select(t => new
                    {
                        t.Id,
                        t.Name,
                        NotesCount = t.NoteTags.Count(nt => nt.Note.UserId == userId && !nt.Note.IsDeleted)
                    })
                    .ToListAsync(ct);

                var tags = tagData.Select(t => new TagItem(t.Id, t.Name, t.NotesCount)).ToList();
                logger.LogInformation("Retrieved {Count} tags for user {UserId}", tags.Count, userId);

                var response = new Response(tags);
                return Results.Ok(response);
            }
        }
    }
}
