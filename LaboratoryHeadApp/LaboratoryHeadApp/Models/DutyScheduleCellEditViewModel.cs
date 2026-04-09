namespace LaboratoryHeadApp.Models
{
    public class DutyScheduleCellEditViewModel
    {
        public DateTime Date { get; set; }

        public int LessonTimeId { get; set; }

        public int PairNumber { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int? DutyPerson1Id { get; set; }

        public int? DutyPerson2Id { get; set; }
    }
}
