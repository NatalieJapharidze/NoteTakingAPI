using System.Security.Claims;
using FluentValidation;
using NoteTakingAPI.Infrastructure.Database;
using NoteTakingAPI.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using NoteTakingAPI.Infrastructure.Database.Entities;

namespace NoteTakingAPI.Features.Notes
{
    public class UpdateNote
    {
        public record Command(int Id, string Title, string Content, List<string> Tags);
        public record Response(int Id, string Title, string Content, List<string> Tags, DateTime UpdatedAt);

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
                RuleFor(x => x.Content).NotEmpty();
                RuleFor(x => x.Tags).Must(tags => tags.All(tag => !string.IsNullOrWhiteSpace(tag)))
                    .WithMessage("Tag names cannot be empty");
            }
        }

        public class Endpoint
        {
            public static void Map(IEndpointRouteBuilder app) =>
                app.MapPut("/notes/{id}", Handle)
                   .RequireAuthorization()
                   .WithOpenApi()
                   .WithSummary("Update note")
                   .WithDescription("Updates an existing note");

            static async Task<IResult> Handle(
                int id,
                Command command,
                ClaimsPrincipal user,
                AppDbContext db,
                IValidator<Command> validator,
                ILogger<UpdateNote> logger,
                CancellationToken ct)
            {
                var validationResult = await validator.ValidateAsync(command, ct);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var userId = user.GetUserId();

                var note = await db.Notes
                    .Where(n => n.Id == id && n.UserId == userId && !n.IsDeleted)
                    .Include(n => n.NoteTags)
                    .FirstOrDefaultAsync(ct);

                if (note is null)
                {
                    logger.LogWarning("Note {NoteId} not found for user {UserId}", id, userId);
                    return Results.NotFound();
                }

                note.Title = command.Title;
                note.Content = command.Content;
                note.UpdatedAt = DateTime.UtcNow;

                db.NoteTags.RemoveRange(note.NoteTags);

                var tagNames = command.Tags.Distinct().ToList();
                var existingTags = await db.Tags
                    .Where(t => tagNames.Contains(t.Name))
                    .ToListAsync(ct);

                var newTagNames = tagNames.Except(existingTags.Select(t => t.Name)).ToList();
                var newTags = newTagNames.Select(name => new Tag { Name = name }).ToList();

                db.Tags.AddRange(newTags);
                await db.SaveChangesAsync(ct);

                var allTags = existingTags.Concat(newTags).ToList();
                var noteTags = allTags.Select(tag => new NoteTag { NoteId = note.Id, TagId = tag.Id }).ToList();

                db.NoteTags.AddRange(noteTags);
                await db.SaveChangesAsync(ct);

                logger.LogInformation("Note {NoteId} updated by user {UserId}", note.Id, userId);

                var response = new Response(note.Id, note.Title, note.Content, tagNames, note.UpdatedAt);
                return Results.Ok(response);
            }
        }
    }
}
