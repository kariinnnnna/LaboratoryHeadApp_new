using Microsoft.AspNetCore.Mvc;
using MolServiceContracts.BindingModels;
using MOLServiceWebClient;

namespace LaboratoryHeadApp.Controllers
{
    public class MolController : Controller
    {
        private readonly IMolApiClient _molApiClient;

        public MolController(IMolApiClient molApiClient)
        {
            _molApiClient = molApiClient;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportClassroomsFromCore()
        {
            try
            {
                var result = await _molApiClient.ImportClassroomsFromCoreAsync();

                TempData["SuccessMessage"] = result
                    ? "Синхронизация аудиторий из Core System успешно выполнена."
                    : "Не удалось выполнить синхронизацию аудиторий.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при синхронизации аудиторий: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportInventoryFromOneC(OneCImportBindingModel model)
        {
            try
            {
                model ??= new OneCImportBindingModel();

                var result = await _molApiClient.ImportInventoryFromOneCAsync(model);

                if (result == null)
                {
                    TempData["ErrorMessage"] = "Синхронизация с 1С завершилась без результата.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["SuccessMessage"] =
                    $"Синхронизация с 1С выполнена. " +
                    $"Обработано: {result.ImportedCount}. " +
                    $"Создано: {result.CreatedCount}. " +
                    $"Обновлено: {result.UpdatedCount}. " +
                    $"Ошибок: {result.ErrorCount}.";

                if (result.Errors.Any())
                {
                    TempData["ErrorMessage"] =
                        $"Во время синхронизации с 1С возникли ошибки: {result.ErrorCount}. " +
                        $"Первые ошибки: {string.Join("; ", result.Errors.Take(3))}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при синхронизации с 1С: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}