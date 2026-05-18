using LaboratoryHeadApp.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MolServiceContracts.BindingModels;
using MolServiceContracts.SearchModels;
using MolServiceContracts.ViewModels;
using MolServiceDataModels.Enums;
using MOLServiceWebClient;

namespace LaboratoryHeadApp.Controllers
{
    public class MaterialTechnicalValueController : Controller
    {
        private readonly IMolApiClient _client;

        public MaterialTechnicalValueController(IMolApiClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            MaterialTechnicalValueSourceType sourceType = MaterialTechnicalValueSourceType.FixedAsset,
            int page = 1,
            int pageSize = 20,
            string? searchText = null)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 20;
            }

            var pagedResult = await _client.GetMaterialTechnicalValuesPagedAsync(
                new MaterialTechnicalValueSearchModel
                {
                    SourceType = sourceType,
                    Page = page,
                    PageSize = pageSize,
                    SearchText = searchText
                });

            if (pagedResult == null)
            {
                TempData["ErrorMessage"] = "Не удалось получить список МТЦ.";

                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = 0;
                ViewBag.TotalCount = 0;
                ViewBag.SourceType = sourceType;
                ViewBag.SearchText = searchText ?? string.Empty;

                return View(new List<MaterialTechnicalValueViewModel>());
            }

            ViewBag.CurrentPage = pagedResult.Page;
            ViewBag.PageSize = pagedResult.PageSize;
            ViewBag.TotalPages = pagedResult.TotalPages;
            ViewBag.TotalCount = pagedResult.TotalCount;
            ViewBag.SourceType = sourceType;
            ViewBag.SearchText = searchText ?? string.Empty;

            return View(pagedResult.Items);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var element = await _client.GetMaterialTechnicalValueAsync(id);

            if (element == null)
            {
                return NotFound();
            }

            var softwareRecords =
                await _client.GetSoftwareRecordsByMaterialTechnicalValueAsync(id)
                ?? new List<SoftwareRecordViewModel>();

            ViewBag.SoftwareRecords = softwareRecords;

            var canInstallSoftware =
                SoftwareInstallRuleHelper.CanInstallSoftware(element) &&
                element.Quantity > 0;

            ViewBag.CanInstallSoftware = canInstallSoftware;
            ViewBag.SoftwareRestrictionReason = canInstallSoftware
                ? null
                : SoftwareInstallRuleHelper.GetRestrictionReason(element);

            return View(element);
        }

        [HttpGet]
        public async Task<IActionResult> Create(
            MaterialTechnicalValueSourceType sourceType = MaterialTechnicalValueSourceType.FixedAsset)
        {
            await LoadDictionariesAsync();

            return View(new MaterialTechnicalValueBindingModel
            {
                SourceType = sourceType,
                Location = "Кафедра ИС",
                ExternalKey = string.Empty
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(MaterialTechnicalValueBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDictionariesAsync();
                return View(model);
            }

            model.Location = string.IsNullOrWhiteSpace(model.Location)
                ? "Кафедра ИС"
                : model.Location.Trim();

            model.ExternalKey ??= string.Empty;

            var result = await _client.CreateMaterialTechnicalValueAsync(model);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Не удалось создать МТЦ");
                await LoadDictionariesAsync();
                return View(model);
            }

            return RedirectToAction(nameof(Index), new
            {
                sourceType = model.SourceType
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var element = await _client.GetMaterialTechnicalValueAsync(id);

            if (element == null)
            {
                return NotFound();
            }

            var model = new MaterialTechnicalValueBindingModel
            {
                Id = element.Id,
                InventoryNumber = element.InventoryNumber,
                ClassroomId = element.ClassroomId,
                FullName = element.FullName,
                Quantity = element.Quantity,
                Description = element.Description,
                Location = element.Location,
                MaterialResponsiblePersonId = element.MaterialResponsiblePersonId,
                SourceType = element.SourceType,
                ExternalKey = element.ExternalKey
            };

            await LoadDictionariesAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MaterialTechnicalValueBindingModel model)
        {
            var current = await _client.GetMaterialTechnicalValueAsync(model.Id);

            if (current == null)
            {
                return NotFound();
            }

            model.SourceType = current.SourceType;
            model.ExternalKey = current.ExternalKey;

            model.Location = string.IsNullOrWhiteSpace(model.Location)
                ? "Кафедра ИС"
                : model.Location.Trim();

            if (!ModelState.IsValid)
            {
                await LoadDictionariesAsync();
                return View(model);
            }

            var result = await _client.UpdateMaterialTechnicalValueAsync(model);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Не удалось обновить МТЦ");
                await LoadDictionariesAsync();
                return View(model);
            }

            return RedirectToAction(nameof(Index), new
            {
                sourceType = current.SourceType
            });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(
            int id,
            MaterialTechnicalValueSourceType sourceType = MaterialTechnicalValueSourceType.FixedAsset)
        {
            var result = await _client.DeleteMaterialTechnicalValueAsync(id);

            if (!result)
            {
                TempData["ErrorMessage"] = "Не удалось удалить МТЦ";
            }

            return RedirectToAction(nameof(Index), new
            {
                sourceType
            });
        }

        [HttpGet]
        public async Task<IActionResult> AssignClassroom(int id)
        {
            var element = await _client.GetMaterialTechnicalValueAsync(id);

            if (element == null)
            {
                return NotFound();
            }

            var classrooms = await _client.GetClassroomsAsync() ?? new();

            ViewBag.Classrooms = classrooms
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Number
                })
                .ToList();

            var model = new MaterialTechnicalValueBindingModel
            {
                Id = element.Id,
                InventoryNumber = element.InventoryNumber,
                FullName = element.FullName,
                Quantity = element.Quantity,
                Description = element.Description,
                Location = element.Location,
                MaterialResponsiblePersonId = element.MaterialResponsiblePersonId,
                ClassroomId = element.ClassroomId,
                SourceType = element.SourceType,
                ExternalKey = element.ExternalKey
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignClassroom(MaterialTechnicalValueBindingModel model)
        {
            var current = await _client.GetMaterialTechnicalValueAsync(model.Id);

            if (current == null)
            {
                return NotFound();
            }

            var updateModel = new MaterialTechnicalValueBindingModel
            {
                Id = current.Id,
                InventoryNumber = current.InventoryNumber,
                FullName = current.FullName,
                Quantity = current.Quantity,
                Description = current.Description,
                Location = string.IsNullOrWhiteSpace(current.Location)
                    ? "Кафедра ИС"
                    : current.Location,
                MaterialResponsiblePersonId = current.MaterialResponsiblePersonId,
                ClassroomId = model.ClassroomId,
                SourceType = current.SourceType,
                ExternalKey = current.ExternalKey
            };

            var result = await _client.UpdateMaterialTechnicalValueAsync(updateModel);

            if (!result)
            {
                var classrooms = await _client.GetClassroomsAsync() ?? new();

                ViewBag.Classrooms = classrooms
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Number
                    })
                    .ToList();

                ModelState.AddModelError(string.Empty, "Не удалось привязать аудиторию");

                return View(model);
            }

            return RedirectToAction(nameof(Details), new
            {
                id = model.Id
            });
        }

        private async Task LoadDictionariesAsync()
        {
            var classrooms = await _client.GetClassroomsAsync() ?? new();
            var responsiblePersons =
                await _client.GetMaterialResponsiblePersonsAsync() ?? new();

            ViewBag.Classrooms = classrooms
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Number
                })
                .ToList();

            ViewBag.MaterialResponsiblePersons = responsiblePersons
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.FullName
                })
                .ToList();
        }
    }
}