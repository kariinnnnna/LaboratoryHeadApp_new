namespace LaboratoryHeadApp.Models
{
    public class ScheduleItemApiModel
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Subject { get; set; } = string.Empty;

        public int? LessonTimeId { get; set; }
        public int? PairNumber { get; set; }

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public int? ClassroomId { get; set; }
        public string? ClassroomNumber { get; set; }

        public int? GroupId { get; set; }
        public string? GroupName { get; set; }

        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }

        public string? Comment { get; set; }
    }
}
