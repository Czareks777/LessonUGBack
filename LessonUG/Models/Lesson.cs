namespace LessonUG.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public List <User> Users { get; set; }
        public DateTime Date { get; set; }
    }
}
