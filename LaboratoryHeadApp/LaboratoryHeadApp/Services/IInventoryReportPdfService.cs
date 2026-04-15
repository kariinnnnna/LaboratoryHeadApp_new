using MolServiceContracts.ViewModels.Reports;

namespace LaboratoryHeadApp.Services
{
    public interface IInventoryReportPdfService
    {
        byte[] GenerateFullInventoryPdf(FullInventoryReportViewModel model);
        byte[] GenerateClassroomsInventoryPdf(ClassroomsInventoryReportViewModel model);
    }
}
