using ScheduleServiceContracts.ViewModels;

namespace LaboratoryHeadApp.Models
{
    public class DutyScheduleIndexViewModel
    {
        public List<LessonTimeViewModel> LessonTimes { get; set; } = new();

        public List<DateTime> CurrentWeekDates { get; set; } = new();

        public List<DateTime> NextWeekDates { get; set; } = new();

        public Dictionary<string, List<string>> CurrentWeekData { get; set; } = new();

        public Dictionary<string, List<string>> NextWeekData { get; set; } = new();
    }
}
