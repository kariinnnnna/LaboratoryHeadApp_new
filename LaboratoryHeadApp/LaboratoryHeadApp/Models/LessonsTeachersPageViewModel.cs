namespace LaboratoryHeadApp.Models
{
    public class LessonsTeachersPageViewModel
    {
        public string SelectedDate { get; set; } = string.Empty;

        public List<string> Teachers { get; set; } = new();
        public List<string> Groups { get; set; } = new();
        public List<string> Classrooms { get; set; } = new();

        public List<LessonsTeachersItemViewModel> Lessons { get; set; } = new();
    }
}
