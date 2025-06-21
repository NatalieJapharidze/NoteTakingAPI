using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NoteTakingAPI.Common.Extensions;
using NoteTakingAPI.Infrastructure.Database;

namespace NoteTakingAPI.Features.Notes
{
    public class GetNoteById
    {
        public record Query(int Id, int UserId);
        public record Response(int Id, string Title, string Content, List<string> Tags, DateTime CreatedAt, DateTime UpdatedAt);

        public class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapGet("/notes/{id}", Handle)
                   .RequireAuthorization()
                   .WithOpenApi()
                   .WithSummary("Get note by ID")
                   .WithDescription("Retrieves a specific note by its ID");

            static async Task<IResult> Handle(
                int id,
                ClaimsPrincipal user,
                AppDbContext db,
                ILogger<GetNoteById> logger,
                CancellationToken ct)
            {
                var userId = user.GetUserId();

                var note = await db.Notes
                    .Where(n => n.Id == id && n.UserId == userId && !n.IsDeleted)
                    .Include(n => n.NoteTags)
                    .ThenInclude(nt => nt.Tag)
                    .FirstOrDefaultAsync(ct);

                if (note is null)
                {
                    logger.LogWarning("Note {NoteId} not found for user {UserId}", id, userId);
                    return Results.NotFound();
                }

                var response = new Response(
                    note.Id,
                    note.Title,
                    note.Content,
                    note.NoteTags.Select(nt => nt.Tag.Name).ToList(),
                    note.CreatedAt,
                    note.UpdatedAt
                );

                return Results.Ok(response);
            }
        }
    }
}
