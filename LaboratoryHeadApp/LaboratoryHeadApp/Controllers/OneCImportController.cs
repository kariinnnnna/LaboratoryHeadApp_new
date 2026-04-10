using Microsoft.AspNetCore.Mvc;
using MolServiceContracts.BindingModels;
using MOLServiceWebClient;

namespace LaboratoryHeadApp.Controllers
{
    public class OneCImportController : Controller
    {
        private readonly IMolApiClient _client;

        public OneCImportController(IMolApiClient client)
        {
            _client = client;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportInventory(OneCImportBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Введите логин и пароль для синхронизации с 1С.";
                TempData["OpenImportModal"] = "true";
                return RedirectToAction("Index", "MaterialTechnicalValue");
            }

            try
            {
                var result = await _client.ImportInventoryFromOneCAsync(model);

                if (result == null)
                {
                    TempData["ErrorMessage"] = "Не удалось выполнить импорт. Проверьте доступ к сети университета и корректность введённых данных.";
                    TempData["OpenImportModal"] = "true";
                    return RedirectToAction("Index", "MaterialTechnicalValue");
                }

                TempData["SuccessMessage"] =
                    $"Импорт завершён. Обработано: {result.ImportedCount}, создано: {result.CreatedCount}, обновлено: {result.UpdatedCount}, ошибок: {result.ErrorCount}.";

                return RedirectToAction("Index", "MaterialTechnicalValue");
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Не удалось связаться с сервисом импорта. Проверьте, запущен ли MolServiceRestApi.";
                TempData["OpenImportModal"] = "true";
                return RedirectToAction("Index", "MaterialTechnicalValue");
            }
            catch (TaskCanceledException)
            {
                TempData["ErrorMessage"] = "Запрос на синхронизацию был прерван или выполнялся слишком долго.";
                TempData["OpenImportModal"] = "true";
                return RedirectToAction("Index", "MaterialTechnicalValue");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                TempData["OpenImportModal"] = "true";
                return RedirectToAction("Index", "MaterialTechnicalValue");
            }
        }
    }
}