namespace CloudBackend.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string CardId { get; set; }
        public string Image { get; set; }
        public List<Event> Events { get; set; }
    }
}
