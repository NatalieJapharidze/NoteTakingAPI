using System.Security.Claims;
using FluentValidation;
using NoteTakingAPI.Infrastructure.Database;
using NoteTakingAPI.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using NoteTakingAPI.Infrastructure.Database.Entities;

namespace NoteTakingAPI.Features.Notes
{
    public class CreateNote
    {
        public record Command(string Title, string Content, List<string> Tags);
        public record Response(int Id, string Title, string Content, List<string> Tags, DateTime CreatedAt);

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
                app.MapPost("/notes", Handle)
                   .RequireAuthorization()
                   .WithOpenApi()
                   .WithSummary("Create new note")
                   .WithDescription("Creates a new note with title, content and tags");

            static async Task<IResult> Handle(
                Command command,
                ClaimsPrincipal user,
                AppDbContext db,
                IValidator<Command> validator,
                ILogger<CreateNote> logger,
                CancellationToken ct)
            {
                var validationResult = await validator.ValidateAsync(command, ct);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var userId = user.GetUserId();

                var note = new Note
                {
                    UserId = userId,
                    Title = command.Title,
                    Content = command.Content
                };

                db.Notes.Add(note);
                await db.SaveChangesAsync(ct);

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

                logger.LogInformation("Note {NoteId} created by user {UserId}", note.Id, userId);

                var response = new Response(note.Id, note.Title, note.Content, tagNames, note.CreatedAt);
                return Results.Created($"/notes/{note.Id}", response);
            }
        }
    }
}
