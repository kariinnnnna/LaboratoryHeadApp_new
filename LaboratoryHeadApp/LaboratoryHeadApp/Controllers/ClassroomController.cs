using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MolServiceContracts.BindingModels;
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
        public async Task<IActionResult> Index()
        {
            var result = await _client.GetClassroomsAsync();
            return View(result ?? new List<MolServiceContracts.ViewModels.ClassroomViewModel>());
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
                    TempData["SuccessMessage"] = "Синхронизация аудиторий из core успешно выполнена.";
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
