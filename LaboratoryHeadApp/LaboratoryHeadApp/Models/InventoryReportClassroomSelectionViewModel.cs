namespace LaboratoryHeadApp.Models
{
    public class InventoryReportClassroomSelectionViewModel
    {
        public List<InventoryReportClassroomItemViewModel> Classrooms { get; set; } = new();
    }

    public class InventoryReportClassroomItemViewModel
    {
        public int ClassroomId { get; set; }
        public string ClassroomNumber { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
