using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScheduleServiceContracts.BindingModels;

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
        public async Task<IActionResult> Index()
        {
            try
            {
                var groups = await _scheduleApiClient.GetGroupsAsync();
                return View(groups ?? new List<ScheduleServiceContracts.ViewModels.GroupViewModel>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при получении списка групп: {ex.Message}";
                return View(new List<ScheduleServiceContracts.ViewModels.GroupViewModel>());
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
