using MolServiceContracts.BindingModels;
using MolServiceContracts.ViewModels;

namespace MOLServiceWebClient
{
    public interface IMolApiClient
    {
        Task<List<ClassroomViewModel>?> GetClassroomsAsync();

        Task<ClassroomViewModel?> GetClassroomAsync(int id);

        Task<bool> CreateClassroomAsync(ClassroomBindingModel model);

        Task<bool> UpdateClassroomAsync(ClassroomBindingModel model);

        Task<bool> DeleteClassroomAsync(int id);

        Task<List<MaterialResponsiblePersonViewModel>?> GetMaterialResponsiblePersonsAsync();

        Task<MaterialResponsiblePersonViewModel?> GetMaterialResponsiblePersonAsync(int id);

        Task<bool> CreateMaterialResponsiblePersonAsync(MaterialResponsiblePersonBindingModel model);

        Task<bool> UpdateMaterialResponsiblePersonAsync(MaterialResponsiblePersonBindingModel model);

        Task<bool> DeleteMaterialResponsiblePersonAsync(int id);

        Task<List<SoftwareViewModel>> GetSoftwaresAsync();
        Task<bool> CreateSoftwareAsync(SoftwareBindingModel model);
        Task<SoftwareViewModel> GetSoftwareAsync(int id);
        Task<bool> UpdateSoftwareAsync(SoftwareBindingModel model);
        Task<bool> DeleteSoftwareAsync(int id);

        Task<OneCImportResultViewModel?> ImportInventoryFromOneCAsync(OneCImportBindingModel model);

        Task<List<MaterialTechnicalValueViewModel>?> GetMaterialTechnicalValuesAsync();
        Task<MaterialTechnicalValueViewModel?> GetMaterialTechnicalValueAsync(int id);
        Task<bool> CreateMaterialTechnicalValueAsync(MaterialTechnicalValueBindingModel model);
        Task<bool> UpdateMaterialTechnicalValueAsync(MaterialTechnicalValueBindingModel model);
        Task<bool> DeleteMaterialTechnicalValueAsync(int id);
        Task<bool> CreateEquipmentMovementHistoryAsync(EquipmentMovementHistoryBindingModel model);
        Task<List<EquipmentMovementHistoryViewModel>?> GetEquipmentMovementHistoriesAsync();

        Task<List<SoftwareRecordViewModel>?> GetSoftwareRecordsAsync();
        Task<List<SoftwareRecordViewModel>?> GetSoftwareRecordsByMaterialTechnicalValueAsync(int materialTechnicalValueId);
        Task<SoftwareRecordViewModel?> GetSoftwareRecordAsync(int id);
        Task<bool> CreateSoftwareRecordAsync(SoftwareRecordBindingModel model);
        Task<bool> UpdateSoftwareRecordAsync(SoftwareRecordBindingModel model);
        Task<bool> DeleteSoftwareRecordAsync(int id);

        Task<bool> ImportClassroomsFromCoreAsync();
        Task<SoftwareAssignToClassroomResultViewModel?> AssignSoftwareToClassroomAsync(SoftwareAssignToClassroomBindingModel model);
    }
}