namespace NoteTakingAPI.Infrastructure.Database.Entities
{
    public class NoteTag
    {
        public int NoteId { get; set; }
        public int TagId { get; set; }

        public Note Note { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}
