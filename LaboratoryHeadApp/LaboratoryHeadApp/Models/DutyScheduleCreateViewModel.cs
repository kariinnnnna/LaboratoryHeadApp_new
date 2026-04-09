using Microsoft.AspNetCore.Mvc.Rendering;
using ScheduleServiceContracts.ViewModels;

namespace LaboratoryHeadApp.Models
{
    public class DutyScheduleCreateViewModel
    {
        public List<DutyScheduleCellEditViewModel> Cells { get; set; } = new();

        public List<DutyPersonViewModel> DutyPersons { get; set; } = new();

        public List<LessonTimeViewModel> LessonTimes { get; set; } = new();

        public List<SelectListItem> DutyPersonItems { get; set; } = new();
    }
}
