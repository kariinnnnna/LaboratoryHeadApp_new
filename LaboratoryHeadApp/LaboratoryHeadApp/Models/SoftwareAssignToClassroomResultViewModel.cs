namespace LaboratoryHeadApp.Models
{
    public class SoftwareAssignToClassroomResultViewModel
    {
        public int ClassroomId { get; set; }
        public int SoftwareId { get; set; }

        public int FoundEquipmentCount { get; set; }
        public int EligibleEquipmentCount { get; set; }
        public int CreatedCount { get; set; }
        public int SkippedDuplicatesCount { get; set; }

        public List<string> Errors { get; set; } = new();
    }
}
