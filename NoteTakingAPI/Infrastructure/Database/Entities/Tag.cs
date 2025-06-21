namespace NoteTakingAPI.Infrastructure.Database.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
    }
}
