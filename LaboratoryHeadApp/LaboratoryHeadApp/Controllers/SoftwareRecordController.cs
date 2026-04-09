using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MolServiceContracts.BindingModels;
using MolServiceContracts.ViewModels;
using MOLServiceWebClient;

namespace LaboratoryHeadApp.Controllers
{
    public class SoftwareRecordController : Controller
    {
        private readonly IMolApiClient _client;

        public SoftwareRecordController(IMolApiClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int materialTechnicalValueId)
        {
            var equipment = await _client.GetMaterialTechnicalValueAsync(materialTechnicalValueId);
            if (equipment == null)
            {
                return NotFound();
            }

            var softwares = await _client.GetSoftwaresAsync() ?? new List<SoftwareViewModel>();

            ViewBag.MaterialTechnicalValueName = equipment.FullName;
            ViewBag.InventoryNumber = equipment.InventoryNumber;
            ViewBag.ClassroomNumber = equipment.ClassroomNumber;
            ViewBag.Softwares = softwares.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.SoftwareName
            }).ToList();

            var model = new SoftwareRecordBindingModel
            {
                MaterialTechnicalValueId = equipment.Id
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SoftwareRecordBindingModel model)
        {
            var equipment = await _client.GetMaterialTechnicalValueAsync(model.MaterialTechnicalValueId);
            if (equipment == null)
            {
                return NotFound();
            }

            var softwares = await _client.GetSoftwaresAsync() ?? new List<MolServiceContracts.ViewModels.SoftwareViewModel>();

            ViewBag.MaterialTechnicalValueName = equipment.FullName;
            ViewBag.InventoryNumber = equipment.InventoryNumber;
            ViewBag.ClassroomNumber = equipment.ClassroomNumber;
            ViewBag.Softwares = softwares.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.SoftwareName
            }).ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.SoftwareId <= 0)
            {
                ModelState.AddModelError(nameof(model.SoftwareId), "Выберите программное обеспечение");
                return View(model);
            }

            try
            {
                var result = await _client.CreateSoftwareRecordAsync(model);
                if (!result)
                {
                    ModelState.AddModelError(string.Empty, "Не удалось привязать ПО");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            return RedirectToAction("Details", "MaterialTechnicalValue", new { id = model.MaterialTechnicalValueId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int materialTechnicalValueId)
        {
            try
            {
                var result = await _client.DeleteSoftwareRecordAsync(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Не удалось удалить привязку ПО";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Details", "MaterialTechnicalValue", new { id = materialTechnicalValueId });
        }
    }
}
