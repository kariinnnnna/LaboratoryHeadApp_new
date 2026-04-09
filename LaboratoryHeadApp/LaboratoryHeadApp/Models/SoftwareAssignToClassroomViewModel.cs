using Microsoft.AspNetCore.Mvc.Rendering;
using MolServiceContracts.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace LaboratoryHeadApp.Models
{
    public class SoftwareAssignToClassroomViewModel
    {
        [Required(ErrorMessage = "Выберите аудиторию")]
        [Display(Name = "Аудитория")]
        public int ClassroomId { get; set; }

        [Required(ErrorMessage = "Выберите программное обеспечение")]
        [Display(Name = "Программное обеспечение")]
        public int SoftwareId { get; set; }

        [Display(Name = "Описание установки")]
        public string SetupDescription { get; set; } = string.Empty;

        [Display(Name = "Номер заявки")]
        public string ClaimNumber { get; set; } = string.Empty;

        public List<SelectListItem> Classrooms { get; set; } = new();
        public List<SelectListItem> Softwares { get; set; } = new();

        public SoftwareAssignToClassroomResultViewModel? Result { get; set; }
    }
}
