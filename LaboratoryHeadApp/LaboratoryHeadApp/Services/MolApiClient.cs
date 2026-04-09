using System.Net.Http.Json;
using MolServiceContracts.BindingModels;
using MolServiceContracts.SearchModels;
using MolServiceContracts.ViewModels;

namespace MOLServiceWebClient
{
    public class MolApiClient : IMolApiClient
    {
        private readonly HttpClient _httpClient;

        public MolApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ClassroomViewModel>?> GetClassroomsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ClassroomViewModel>>("api/Classroom/GetAll");
        }

        public async Task<ClassroomViewModel?> GetClassroomAsync(int id)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Classroom/GetElement", new ClassroomSearchModel
            {
                Id = id
            });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ClassroomViewModel>();
        }

        public async Task<bool> CreateClassroomAsync(ClassroomBindingModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Classroom/Create", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateClassroomAsync(ClassroomBindingModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Classroom/Update", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteClassroomAsync(int id)
        {
            var response = await _httpClient.PostAsync($"api/Classroom/Delete?id={id}", null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка при удалении аудитории: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<bool>();
            return result;
        }


        public async Task<List<MaterialResponsiblePersonViewModel>?> GetMaterialResponsiblePersonsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<MaterialResponsiblePersonViewModel>>(
                "api/MaterialResponsiblePerson/GetAll");
        }

        public async Task<MaterialResponsiblePersonViewModel?> GetMaterialResponsiblePersonAsync(int id)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/MaterialResponsiblePerson/GetElement",
                new MaterialResponsiblePersonSearchModel { Id = id });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<MaterialResponsiblePersonViewModel>();
        }

        public async Task<bool> CreateMaterialResponsiblePersonAsync(MaterialResponsiblePersonBindingModel model)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/MaterialResponsiblePerson/Create",
                model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка при создании МОЛ: {error}");
            }

            return true;
        }

        public async Task<bool> UpdateMaterialResponsiblePersonAsync(MaterialResponsiblePersonBindingModel model)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/MaterialResponsiblePerson/Update",
                model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка при обновлении МОЛ: {error}");
            }

            return true;
        }

        public async Task<bool> DeleteMaterialResponsiblePersonAsync(int id)
        {
            var response = await _httpClient.PostAsync(
                $"api/MaterialResponsiblePerson/Delete?id={id}",
                null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка при удалении МОЛ: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<bool>();
            return result;
        }
        public async Task<List<SoftwareViewModel>?> GetSoftwaresAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<SoftwareViewModel>>("api/Software/GetAll");
        }

        // Получить ПО по ID
        public async Task<SoftwareViewModel?> GetSoftwareAsync(int id)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Software/GetElement", new SoftwareSearchModel
            {
                Id = id
            });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<SoftwareViewModel>();
        }

        // Создать новое ПО
        public async Task<bool> CreateSoftwareAsync(SoftwareBindingModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Software/Create", model);
            return response.IsSuccessStatusCode;
        }

        // Обновить данные ПО
        public async Task<bool> UpdateSoftwareAsync(SoftwareBindingModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Software/Update", model);
            return response.IsSuccessStatusCode;
        }

        // Удалить ПО
        public async Task<bool> DeleteSoftwareAsync(int id)
        {
            var response = await _httpClient.PostAsync($"api/Software/Delete?id={id}", null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка при удалении ПО: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<bool>();
            return result;
        }
        public async Task<OneCImportResultViewModel?> ImportInventoryFromOneCAsync(OneCImportBindingModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/OneCImport/ImportInventory", model);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<OneCImportResultViewModel>();
        }
        public async Task<List<MaterialTechnicalValueViewModel>?> GetMaterialTechnicalValuesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<MaterialTechnicalValueViewModel>>(
                "api/MaterialTechnicalValue/GetAll");
        }

        public async Task<MaterialTechnicalValueViewModel?> GetMaterialTechnicalValueAsync(int id)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/MaterialTechnicalValue/GetElement",
                new MaterialTechnicalValueSearchModel { Id = id });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<MaterialTechnicalValueViewModel>();
        }

        public async Task<bool> CreateMaterialTechnicalValueAsync(MaterialTechnicalValueBindingModel model)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/MaterialTechnicalValue/Create",
                model);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateMaterialTechnicalValueAsync(MaterialTechnicalValueBindingModel model)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/MaterialTechnicalValue/Update",
                model);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteMaterialTechnicalValueAsync(int id)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/MaterialTechnicalValue/Delete",
                new MaterialTechnicalValueBindingModel { Id = id });

            return response.IsSuccessStatusCode;
        }
        public async Task<List<EquipmentMovementHistoryViewModel>?> GetEquipmentMovementHistoriesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<EquipmentMovementHistoryViewModel>>(
                "api/EquipmentMovementHistory/GetAll");
        }

        public async Task<List<EquipmentMovementHistoryViewModel>?> GetEquipmentMovementHistoriesByMaterialTechnicalValueAsync(int materialTechnicalValueId)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/EquipmentMovementHistory/GetFiltered",
                new EquipmentMovementHistorySearchModel
                {
                    MaterialTechnicalValueId = materialTechnicalValueId
                });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<List<EquipmentMovementHistoryViewModel>>();
        }

        public async Task<EquipmentMovementHistoryViewModel?> GetEquipmentMovementHistoryAsync(int id)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/EquipmentMovementHistory/GetElement",
                new EquipmentMovementHistorySearchModel
                {
                    Id = id
                });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<EquipmentMovementHistoryViewModel>();
        }

        public async Task<bool> CreateEquipmentMovementHistoryAsync(EquipmentMovementHistoryBindingModel model)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/EquipmentMovementHistory/Create",
                model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка при списании оборудования: {error}");
            }

            return true;
        }

        public async Task<bool> UpdateEquipmentMovementHistoryAsync(EquipmentMovementHistoryBindingModel model)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/EquipmentMovementHistory/Update",
                model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка при обновлении записи списания: {error}");
            }

            return true;
        }

        public async Task<bool> DeleteEquipmentMovementHistoryAsync(int id)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/EquipmentMovementHistory/Delete",
                new EquipmentMovementHistoryBindingModel { Id = id });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка при удалении записи списания: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<bool>();
            return result;
        }
        public async Task<List<SoftwareRecordViewModel>?> GetSoftwareRecordsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<SoftwareRecordViewModel>>(
                "api/SoftwareRecord/GetAll");
        }

        public async Task<List<SoftwareRecordViewModel>?> GetSoftwareRecordsByMaterialTechnicalValueAsync(int materialTechnicalValueId)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/SoftwareRecord/GetFiltered",
                new SoftwareRecordSearchModel
                {
                    MaterialTechnicalValueId = materialTechnicalValueId
                });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<List<SoftwareRecordViewModel>>();
        }

        public async Task<SoftwareRecordViewModel?> GetSoftwareRecordAsync(int id)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/SoftwareRecord/GetElement",
                new SoftwareRecordSearchModel
                {
                    Id = id
                });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<SoftwareRecordViewModel>();
        }

        public async Task<bool> CreateSoftwareRecordAsync(SoftwareRecordBindingModel model)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/SoftwareRecord/Create",
                model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка при привязке ПО: {error}");
            }

            return true;
        }

        public async Task<bool> UpdateSoftwareRecordAsync(SoftwareRecordBindingModel model)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/SoftwareRecord/Update",
                model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка при обновлении записи ПО: {error}");
            }

            return true;
        }

        public async Task<bool> DeleteSoftwareRecordAsync(int id)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/SoftwareRecord/Delete",
                new SoftwareRecordBindingModel { Id = id });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка при удалении записи ПО: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<bool>();
            return result;
        }

        public async Task<bool> ImportClassroomsFromCoreAsync()
        {
            var response = await _httpClient.PostAsync("api/CoreImport/ImportClassrooms", null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка при синхронизации аудиторий: {error}");
            }

            return true;
        }
        public async Task<SoftwareAssignToClassroomResultViewModel?> AssignSoftwareToClassroomAsync(
    SoftwareAssignToClassroomBindingModel model)
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/SoftwareRecord/AssignToClassroom",
                model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            return await response.Content.ReadFromJsonAsync<SoftwareAssignToClassroomResultViewModel>();
        }
    }

}