using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace LaboratoryHeadApp.Models
{
    public class ClassroomReservationCreateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Дата обязательна")]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Display(Name = "Аудитория из справочника")]
        public int? SelectedClassroomId { get; set; }

        [Display(Name = "ID аудитории")]
        public int? ClassroomId { get; set; }

        [Display(Name = "Аудитория")]
        public string? ClassroomNumber { get; set; }

        [Display(Name = "Преподаватель из справочника")]
        public int? SelectedTeacherId { get; set; }

        [Display(Name = "ID преподавателя")]
        public int? TeacherId { get; set; }

        [Display(Name = "Преподаватель")]
        public string? TeacherName { get; set; }

        [Display(Name = "Группа из справочника")]
        public int? SelectedGroupId { get; set; }

        [Display(Name = "ID группы")]
        public int? GroupId { get; set; }

        [Display(Name = "Группа")]
        public string? GroupName { get; set; }

        [Required(ErrorMessage = "Тема обязательна")]
        [Display(Name = "Тема")]
        public string Subject { get; set; } = string.Empty;

        [Display(Name = "Пара")]
        public int? LessonTimeId { get; set; }

        [Display(Name = "Время начала")]
        public TimeSpan? StartTime { get; set; }

        [Display(Name = "Время окончания")]
        public TimeSpan? EndTime { get; set; }

        [Display(Name = "Комментарий")]
        public string? Comment { get; set; }

        public List<SelectListItem> LessonTimes { get; set; } = new();
        public List<SelectListItem> Classrooms { get; set; } = new();
        public List<SelectListItem> Teachers { get; set; } = new();
        public List<SelectListItem> Groups { get; set; } = new();
    }
}
