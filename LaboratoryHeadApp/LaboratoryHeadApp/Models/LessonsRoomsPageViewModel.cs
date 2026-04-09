namespace LaboratoryHeadApp.Models
{
    public class LessonsRoomsPageViewModel
    {
        public string SelectedDate { get; set; } = string.Empty;

        public List<string> Classrooms { get; set; } = new();
        public List<string> Teachers { get; set; } = new();
        public List<string> Groups { get; set; } = new();

        public List<LessonsRoomsItemViewModel> Lessons { get; set; } = new();
    }
}
