using LaboratoryHeadApp.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MolServiceContracts.BindingModels;
using MolServiceContracts.ViewModels;
using MOLServiceWebClient;

namespace LaboratoryHeadApp.Controllers
{
    public class SoftwareRecordController : Controller
    {
        private readonly IMolApiClient _client;

        public SoftwareRecordController(IMolApiClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int materialTechnicalValueId)
        {
            var equipment = await _client.GetMaterialTechnicalValueAsync(materialTechnicalValueId);
            if (equipment == null)
            {
                return NotFound();
            }

            var softwares = await _client.GetSoftwaresAsync() ?? new List<SoftwareViewModel>();

            ViewBag.MaterialTechnicalValueName = equipment.FullName;
            ViewBag.InventoryNumber = equipment.InventoryNumber;
            ViewBag.ClassroomNumber = equipment.ClassroomNumber;
            ViewBag.Softwares = softwares.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.SoftwareName
            }).ToList();

            var model = new SoftwareRecordBindingModel
            {
                MaterialTechnicalValueId = equipment.Id
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SoftwareRecordBindingModel model)
        {
            var equipment = await _client.GetMaterialTechnicalValueAsync(model.MaterialTechnicalValueId);
            if (equipment == null)
            {
                return NotFound();
            }

            var softwares = await _client.GetSoftwaresAsync() ?? new List<SoftwareViewModel>();

            ViewBag.MaterialTechnicalValueName = equipment.FullName;
            ViewBag.InventoryNumber = equipment.InventoryNumber;
            ViewBag.ClassroomNumber = equipment.ClassroomNumber;
            ViewBag.Softwares = softwares.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.SoftwareName
            }).ToList();

            if (equipment.Quantity <= 0)
            {
                ModelState.AddModelError(string.Empty, "Для полностью списанного оборудования установка ПО недоступна.");
                return View(model);
            }

            if (!SoftwareInstallRuleHelper.CanInstallSoftware(equipment))
            {
                ModelState.AddModelError(string.Empty, SoftwareInstallRuleHelper.GetRestrictionReason(equipment));
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.SoftwareId <= 0)
            {
                ModelState.AddModelError(nameof(model.SoftwareId), "Выберите программное обеспечение");
                return View(model);
            }

            try
            {
                var result = await _client.CreateSoftwareRecordAsync(model);
                if (!result)
                {
                    ModelState.AddModelError(string.Empty, "Не удалось привязать ПО");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            return RedirectToAction("Details", "MaterialTechnicalValue", new { id = model.MaterialTechnicalValueId });
        }

        [HttpGet]
        public async Task<IActionResult> AssignToClassroom()
        {
            var model = new SoftwareAssignToClassroomBindingModel();
            await FillAssignToClassroomDictionaries(model.ClassroomId, model.SoftwareId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToClassroom(SoftwareAssignToClassroomBindingModel model)
        {
            await FillAssignToClassroomDictionaries(model.ClassroomId, model.SoftwareId);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.ClassroomId <= 0)
            {
                ModelState.AddModelError(nameof(model.ClassroomId), "Выберите аудиторию");
                return View(model);
            }

            if (model.SoftwareId <= 0)
            {
                ModelState.AddModelError(nameof(model.SoftwareId), "Выберите программное обеспечение");
                return View(model);
            }

            var allEquipment = await _client.GetMaterialTechnicalValuesAsync() ?? new List<MaterialTechnicalValueViewModel>();
            var allSoftwareRecords = await _client.GetSoftwareRecordsAsync() ?? new List<SoftwareRecordViewModel>();

            var classroomEquipment = allEquipment
                .Where(x => x.ClassroomId == model.ClassroomId)
                .ToList();

            var allowedEquipment = classroomEquipment
                .Where(x => x.Quantity > 0)
                .Where(SoftwareInstallRuleHelper.CanInstallSoftware)
                .ToList();

            var result = new SoftwareAssignToClassroomResultViewModel
            {
                ClassroomId = model.ClassroomId,
                SoftwareId = model.SoftwareId,
                FoundEquipmentCount = allowedEquipment.Count
            };

            if (!classroomEquipment.Any())
            {
                ModelState.AddModelError(string.Empty, "В выбранной аудитории нет оборудования.");
                return View(model);
            }

            if (!allowedEquipment.Any())
            {
                ModelState.AddModelError(string.Empty, "В выбранной аудитории нет подходящего активного оборудования для установки ПО.");
                return View(model);
            }

            foreach (var equipment in allowedEquipment)
            {
                try
                {
                    var duplicate = allSoftwareRecords.Any(x =>
                        x.MaterialTechnicalValueId == equipment.Id &&
                        x.SoftwareId == model.SoftwareId);

                    if (duplicate)
                    {
                        result.SkippedDuplicatesCount++;
                        continue;
                    }

                    var created = await _client.CreateSoftwareRecordAsync(new SoftwareRecordBindingModel
                    {
                        MaterialTechnicalValueId = equipment.Id,
                        SoftwareId = model.SoftwareId,
                        SetupDescription = model.SetupDescription?.Trim() ?? string.Empty,
                        ClaimNumber = model.ClaimNumber?.Trim() ?? string.Empty
                    });

                    if (created)
                    {
                        result.CreatedCount++;
                    }
                    else
                    {
                        result.Errors.Add($"{equipment.FullName} ({equipment.InventoryNumber}): не удалось привязать ПО");
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"{equipment.FullName} ({equipment.InventoryNumber}): {ex.Message}");
                }
            }

            ViewBag.AssignResult = result;

            if (result.CreatedCount > 0)
            {
                TempData["SuccessMessage"] = $"ПО успешно назначено. Добавлено: {result.CreatedCount}, пропущено дублей: {result.SkippedDuplicatesCount}.";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int materialTechnicalValueId)
        {
            try
            {
                var result = await _client.DeleteSoftwareRecordAsync(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Не удалось удалить привязку ПО";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Details", "MaterialTechnicalValue", new { id = materialTechnicalValueId });
        }

        private async Task FillAssignToClassroomDictionaries(int selectedClassroomId = 0, int selectedSoftwareId = 0)
        {
            var classrooms = await _client.GetClassroomsAsync() ?? new List<ClassroomViewModel>();
            var softwares = await _client.GetSoftwaresAsync() ?? new List<SoftwareViewModel>();

            ViewBag.Classrooms = classrooms
                .OrderBy(x => x.Number)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Number,
                    Selected = x.Id == selectedClassroomId
                })
                .ToList();

            ViewBag.Softwares = softwares
                .OrderBy(x => x.SoftwareName)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.SoftwareName,
                    Selected = x.Id == selectedSoftwareId
                })
                .ToList();
        }
    }
}