using Microsoft.AspNetCore.Mvc;
using ScheduleServiceContracts.BindingModels;
using ScheduleServiceContracts.ViewModels;

namespace LaboratoryHeadApp.Controllers
{
    public class GroupController : Controller
    {
        private readonly IScheduleApiClient _scheduleApiClient;

        public GroupController(IScheduleApiClient scheduleApiClient)
        {
            _scheduleApiClient = scheduleApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchText,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                var groups = await _scheduleApiClient.GetGroupsAsync()
                    ?? new List<GroupViewModel>();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var search = searchText.Trim().ToLowerInvariant();

                    groups = groups
                        .Where(x =>
                            x.CoreSystemId.ToString().Contains(search) ||
                            (x.GroupName ?? string.Empty).ToLowerInvariant().Contains(search))
                        .ToList();
                }

                page = page < 1 ? 1 : page;
                pageSize = pageSize <= 0 ? 20 : pageSize;

                var totalCount = groups.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                if (totalPages > 0 && page > totalPages)
                {
                    page = totalPages;
                }

                var pagedGroups = groups
                    .OrderBy(x => x.GroupName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.SearchText = searchText ?? string.Empty;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = totalPages;

                return View(pagedGroups);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при получении списка групп: {ex.Message}";

                ViewBag.SearchText = searchText ?? string.Empty;
                ViewBag.Page = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = 0;
                ViewBag.TotalPages = 0;

                return View(new List<GroupViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var group = await _scheduleApiClient.GetGroupAsync(id);

                if (group == null)
                {
                    TempData["ErrorMessage"] = "Группа не найдена";
                    return RedirectToAction(nameof(Index));
                }

                var model = new GroupBindingModel
                {
                    Id = group.Id,
                    CoreSystemId = group.CoreSystemId,
                    GroupName = group.GroupName
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при загрузке группы: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GroupBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _scheduleApiClient.UpdateGroupAsync(model);

                if (!result)
                {
                    TempData["ErrorMessage"] = "Не удалось обновить группу";
                    return View(model);
                }

                TempData["SuccessMessage"] = "Группа успешно обновлена";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при обновлении группы: {ex.Message}";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _scheduleApiClient.DeleteGroupAsync(id);

                TempData["SuccessMessage"] = result
                    ? "Группа успешно удалена"
                    : "Не удалось удалить группу";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при удалении группы: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromCore()
        {
            try
            {
                var result = await _scheduleApiClient.ImportGroupsFromCoreAsync();

                TempData["SuccessMessage"] = result
                    ? "Синхронизация групп успешно выполнена"
                    : "Не удалось выполнить синхронизацию групп";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при синхронизации групп: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}