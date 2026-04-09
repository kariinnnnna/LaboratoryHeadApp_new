using Microsoft.AspNetCore.Http;
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

        [HttpGet]
        public IActionResult ImportInventory()
        {
            return View(new OneCImportBindingModel());
        }

        [HttpPost]
        public async Task<IActionResult> ImportInventory(OneCImportBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _client.ImportInventoryFromOneCAsync(model);

            if (result == null)
            {
                ViewBag.ErrorMessage = "Не удалось выполнить импорт. Проверьте доступ к сети университета и корректность введённых данных.";
                return View(model);
            }

            ViewBag.SuccessMessage = $"Импорт завершён. Обработано: {result.ImportedCount}, создано: {result.CreatedCount}, обновлено: {result.UpdatedCount}, ошибок: {result.ErrorCount}.";
            ViewBag.ImportResult = result;

            return View(new OneCImportBindingModel());
        }
    }
}
