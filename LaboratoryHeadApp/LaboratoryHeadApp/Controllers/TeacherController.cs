using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScheduleServiceContracts.BindingModels;
using ScheduleServiceContracts.SearchModels;
using ScheduleServiceContracts.ViewModels;

namespace LaboratoryHeadApp.Controllers
{
    public class TeacherController : Controller
    {
        private readonly IScheduleApiClient _scheduleApiClient;

        public TeacherController(IScheduleApiClient scheduleApiClient)
        {
            _scheduleApiClient = scheduleApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var teachers = await _scheduleApiClient.GetTeachersAsync();
                return View(teachers ?? new List<ScheduleServiceContracts.ViewModels.TeacherViewModel>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при получении списка преподавателей: {ex.Message}";
                return View(new List<ScheduleServiceContracts.ViewModels.TeacherViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var teacher = await _scheduleApiClient.GetTeacherAsync(id);
                if (teacher == null)
                {
                    TempData["ErrorMessage"] = "Преподаватель не найден";
                    return RedirectToAction(nameof(Index));
                }

                var model = new TeacherBindingModel
                {
                    Id = teacher.Id,
                    CoreSystemId = teacher.CoreSystemId,
                    TeacherName = teacher.TeacherName
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при загрузке преподавателя: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TeacherBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _scheduleApiClient.UpdateTeacherAsync(model);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Не удалось обновить преподавателя";
                    return View(model);
                }

                TempData["SuccessMessage"] = "Преподаватель успешно обновлён";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при обновлении преподавателя: {ex.Message}";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _scheduleApiClient.DeleteTeacherAsync(id);
                TempData["SuccessMessage"] = result
                    ? "Преподаватель успешно удалён"
                    : "Не удалось удалить преподавателя";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при удалении преподавателя: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromCore()
        {
            try
            {
                var result = await _scheduleApiClient.ImportTeachersFromCoreAsync();
                TempData["SuccessMessage"] = result
                    ? "Синхронизация преподавателей успешно выполнена"
                    : "Не удалось выполнить синхронизацию преподавателей";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при синхронизации преподавателей: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
