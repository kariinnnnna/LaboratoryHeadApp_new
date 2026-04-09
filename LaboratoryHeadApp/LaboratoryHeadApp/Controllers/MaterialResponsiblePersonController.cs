using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MolServiceContracts.BindingModels;
using MolServiceContracts.ViewModels;
using MOLServiceWebClient;

namespace LaboratoryHeadApp.Controllers
{
    public class MaterialResponsiblePersonController : Controller
    {
        private readonly IMolApiClient _client;

        public MaterialResponsiblePersonController(IMolApiClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _client.GetMaterialResponsiblePersonsAsync();
            return View(result ?? new List<MaterialResponsiblePersonViewModel>());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new MaterialResponsiblePersonBindingModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(MaterialResponsiblePersonBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _client.CreateMaterialResponsiblePersonAsync(model);
                if (!result)
                {
                    ModelState.AddModelError(string.Empty, "Не удалось создать МОЛ");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var element = await _client.GetMaterialResponsiblePersonAsync(id);
            if (element == null)
            {
                return NotFound();
            }

            var model = new MaterialResponsiblePersonBindingModel
            {
                Id = element.Id,
                FullName = element.FullName,
                Position = element.Position,
                Phone = element.Phone,
                Email = element.Email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MaterialResponsiblePersonBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _client.UpdateMaterialResponsiblePersonAsync(model);
                if (!result)
                {
                    ModelState.AddModelError(string.Empty, "Не удалось обновить МОЛ");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _client.DeleteMaterialResponsiblePersonAsync(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Не удалось удалить МОЛ";
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
