using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MolServiceContracts.BindingModels;
using MolServiceContracts.ViewModels;
using MOLServiceWebClient;

namespace LaboratoryHeadApp.Controllers
{
    public class EquipmentMovementHistoryController : Controller
    {
        private readonly IMolApiClient _client;

        public EquipmentMovementHistoryController(IMolApiClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _client.GetEquipmentMovementHistoriesAsync();
            return View(items ?? new List<EquipmentMovementHistoryViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> Create(int materialTechnicalValueId)
        {
            var equipment = await _client.GetMaterialTechnicalValueAsync(materialTechnicalValueId);
            if (equipment == null)
            {
                return NotFound();
            }

            ViewBag.MaterialTechnicalValueName = equipment.FullName;
            ViewBag.InventoryNumber = equipment.InventoryNumber;
            ViewBag.CurrentQuantity = equipment.Quantity;
            ViewBag.ClassroomNumber = equipment.ClassroomNumber;

            var model = new EquipmentMovementHistoryBindingModel
            {
                MaterialTechnicalValueId = equipment.Id,
                MoveDate = DateTime.Now
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EquipmentMovementHistoryBindingModel model)
        {
            var equipment = await _client.GetMaterialTechnicalValueAsync(model.MaterialTechnicalValueId);
            if (equipment == null)
            {
                return NotFound();
            }

            ViewBag.MaterialTechnicalValueName = equipment.FullName;
            ViewBag.InventoryNumber = equipment.InventoryNumber;
            ViewBag.CurrentQuantity = equipment.Quantity;
            ViewBag.ClassroomNumber = equipment.ClassroomNumber;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Quantity <= 0)
            {
                ModelState.AddModelError(nameof(model.Quantity), "Количество для списания должно быть больше 0");
                return View(model);
            }

            if (model.Quantity > equipment.Quantity)
            {
                ModelState.AddModelError(nameof(model.Quantity), "Нельзя списать больше, чем есть в наличии");
                return View(model);
            }

            try
            {
                var result = await _client.CreateEquipmentMovementHistoryAsync(model);
                if (!result)
                {
                    ModelState.AddModelError(string.Empty, "Не удалось выполнить списание оборудования");
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
    }
}
