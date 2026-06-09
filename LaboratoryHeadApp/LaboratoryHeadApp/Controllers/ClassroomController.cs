using Microsoft.AspNetCore.Mvc;
using MolServiceContracts.BindingModels;
using MolServiceContracts.ViewModels;
using MOLServiceWebClient;

namespace LaboratoryHeadApp.Controllers
{
    public class ClassroomController : Controller
    {
        private readonly IMolApiClient _client;

        public ClassroomController(IMolApiClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchText,
            int page = 1,
            int pageSize = 20)
        {
            var result = await _client.GetClassroomsAsync()
                ?? new List<ClassroomViewModel>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var search = searchText.Trim().ToLowerInvariant();

                result = result
                    .Where(x =>
                        x.CoreSystemId.ToString().Contains(search) ||
                        (x.Number ?? string.Empty).ToLowerInvariant().Contains(search) ||
                        (x.TypeName ?? string.Empty).ToLowerInvariant().Contains(search) ||
                        x.Capacity.ToString().Contains(search) ||
                        (x.NotUseInSchedule ? "да" : "нет").Contains(search) ||
                        (x.HasProjector ? "да" : "нет").Contains(search))
                    .ToList();
            }

            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : pageSize;

            var totalCount = result.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var pagedResult = result
                .OrderBy(x => x.Number)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SearchText = searchText ?? string.Empty;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;

            return View(pagedResult);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ClassroomBindingModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(ClassroomBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _client.CreateClassroomAsync(model);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Не удалось создать аудиторию");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var element = await _client.GetClassroomAsync(id);

            if (element == null)
            {
                return NotFound();
            }

            var model = new ClassroomBindingModel
            {
                Id = element.Id,
                CoreSystemId = element.CoreSystemId,
                Number = element.Number,
                Type = element.Type,
                Capacity = element.Capacity,
                NotUseInSchedule = element.NotUseInSchedule,
                HasProjector = element.HasProjector
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ClassroomBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _client.UpdateClassroomAsync(model);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Не удалось обновить аудиторию");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _client.DeleteClassroomAsync(id);

                if (!result)
                {
                    TempData["ErrorMessage"] = "Не удалось удалить аудиторию";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ImportFromCore()
        {
            try
            {
                var result = await _client.ImportClassroomsFromCoreAsync();

                if (result)
                {
                    TempData["SuccessMessage"] = "Синхронизация аудиторий из Core System успешно выполнена.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Не удалось выполнить синхронизацию аудиторий.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}