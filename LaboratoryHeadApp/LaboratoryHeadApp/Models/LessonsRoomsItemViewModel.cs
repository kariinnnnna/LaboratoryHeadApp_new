namespace LaboratoryHeadApp.Models
{
    public class LessonsRoomsItemViewModel
    {
        public string ClassroomNumber { get; set; } = string.Empty;
        public int? PairNumber { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string Subgroup { get; set; } = string.Empty;
        public bool IsImported { get; set; }
    }
}
