using LaboratoryHeadApp.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MolServiceContracts.BindingModels;
using MolServiceContracts.ViewModels;
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
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            var allItems = await _client.GetMaterialTechnicalValuesAsync()
                          ?? new List<MolServiceContracts.ViewModels.MaterialTechnicalValueViewModel>();

            var totalCount = allItems.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (page < 1)
            {
                page = 1;
            }

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var items = allItems
                .OrderBy(x => x.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var element = await _client.GetMaterialTechnicalValueAsync(id);
            if (element == null)
            {
                return NotFound();
            }

            var softwareRecords = await _client.GetSoftwareRecordsByMaterialTechnicalValueAsync(id)
                                  ?? new List<SoftwareRecordViewModel>();

            ViewBag.SoftwareRecords = softwareRecords;

            var canInstallSoftware = SoftwareInstallRuleHelper.CanInstallSoftware(element) && element.Quantity > 0;
            ViewBag.CanInstallSoftware = canInstallSoftware;
            ViewBag.SoftwareRestrictionReason = canInstallSoftware
                ? null
                : SoftwareInstallRuleHelper.GetRestrictionReason(element);

            return View(element);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDictionariesAsync();
            return View(new MaterialTechnicalValueBindingModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(MaterialTechnicalValueBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDictionariesAsync();
                return View(model);
            }

            var result = await _client.CreateMaterialTechnicalValueAsync(model);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Не удалось создать МТЦ");
                await LoadDictionariesAsync();
                return View(model);
            }

            return RedirectToAction(nameof(Index));
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
                MaterialResponsiblePersonId = element.MaterialResponsiblePersonId
            };

            await LoadDictionariesAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MaterialTechnicalValueBindingModel model)
        {
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

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _client.DeleteMaterialTechnicalValueAsync(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Не удалось удалить МТЦ";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDictionariesAsync()
        {
            var classrooms = await _client.GetClassroomsAsync() ?? new();
            var responsiblePersons = await _client.GetMaterialResponsiblePersonsAsync() ?? new();

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
                ClassroomId = element.ClassroomId
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
                Location = current.Location,
                MaterialResponsiblePersonId = current.MaterialResponsiblePersonId,
                ClassroomId = model.ClassroomId
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

            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
    }
}
