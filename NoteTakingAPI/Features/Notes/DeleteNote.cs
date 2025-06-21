using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NoteTakingAPI.Common.Extensions;
using NoteTakingAPI.Infrastructure.Database;

namespace NoteTakingAPI.Features.Notes
{
    public class DeleteNote
    {
        public class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapDelete("/notes/{id}", Handle)
                   .RequireAuthorization()
                   .WithOpenApi()
                   .WithSummary("Delete note")
                   .WithDescription("Soft deletes a note");

            static async Task<IResult> Handle(
                int id,
                ClaimsPrincipal user,
                AppDbContext db,
                ILogger<DeleteNote> logger,
                CancellationToken ct)
            {
                var userId = user.GetUserId();

                var note = await db.Notes
                    .Where(n => n.Id == id && n.UserId == userId && !n.IsDeleted)
                    .FirstOrDefaultAsync(ct);

                if (note is null)
                {
                    logger.LogWarning("Note {NoteId} not found for user {UserId}", id, userId);
                    return Results.NotFound();
                }

                note.IsDeleted = true;
                note.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync(ct);

                logger.LogInformation("Note {NoteId} deleted by user {UserId}", note.Id, userId);

                return Results.NoContent();
            }
        }
    }
}
