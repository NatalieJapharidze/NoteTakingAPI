using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NoteTakingAPI.Common.Extensions;
using NoteTakingAPI.Infrastructure.Database;

namespace NoteTakingAPI.Features.Notes
{
    public class GetNotes
    {
        public record Query(int Page = 1, int PageSize = 10, string? Search = null, string? Tag = null);
        public record Response(List<NoteItem> Notes, int TotalCount, int Page, int PageSize);
        public record NoteItem(int Id, string Title, string Content, List<string> Tags, DateTime CreatedAt, DateTime UpdatedAt);

        public class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapGet("/notes", Handle)
                   .RequireAuthorization()
                   .WithOpenApi()
                   .WithSummary("Get user notes")
                   .WithDescription("Retrieves paginated list of user notes with optional search and tag filtering");

            static async Task<IResult> Handle(
                [AsParameters] Query query,
                ClaimsPrincipal user,
                AppDbContext db,
                ILogger<GetNotes> logger,
                CancellationToken ct)
            {
                var userId = user.GetUserId();

                var baseQuery = db.Notes.Where(n => n.UserId == userId && !n.IsDeleted);

                if (!string.IsNullOrEmpty(query.Search))
                {
                    baseQuery = baseQuery.Where(n =>
                        n.Title.Contains(query.Search) ||
                        n.Content.Contains(query.Search));
                }

                if (!string.IsNullOrEmpty(query.Tag))
                {
                    baseQuery = baseQuery.Where(n =>
                        n.NoteTags.Any(nt => nt.Tag.Name == query.Tag));
                }

                var totalCount = await baseQuery.CountAsync(ct);

                var notes = await baseQuery
                    .Include(n => n.NoteTags)
                    .ThenInclude(nt => nt.Tag)
                    .OrderByDescending(n => n.UpdatedAt)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(n => new NoteItem(
                        n.Id,
                        n.Title,
                        n.Content,
                        n.NoteTags.Select(nt => nt.Tag.Name).ToList(),
                        n.CreatedAt,
                        n.UpdatedAt))
                    .ToListAsync(ct);

                logger.LogInformation("Retrieved {Count} notes for user {UserId}", notes.Count, userId);

                var response = new Response(notes, totalCount, query.Page, query.PageSize);
                return Results.Ok(response);
            }
        }
    }
}
