namespace LaboratoryHeadApp.Models
{
    public class LessonsGroupsPageViewModel
    {
        public string SelectedDate { get; set; } = string.Empty;

        public List<string> Groups { get; set; } = new();
        public List<string> Teachers { get; set; } = new();
        public List<string> Classrooms { get; set; } = new();

        public List<LessonsGroupsItemViewModel> Lessons { get; set; } = new();
    }
}
