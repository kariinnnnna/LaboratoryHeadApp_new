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
        public async Task<IActionResult> Index(
            string? searchText,
            int page = 1,
            int pageSize = 20)
        {
            var items = await _client.GetEquipmentMovementHistoriesAsync()
                ?? new List<EquipmentMovementHistoryViewModel>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var search = searchText.Trim().ToLowerInvariant();

                items = items
                    .Where(x =>
                        x.MaterialTechnicalValueId.ToString().Contains(search) ||
                        (x.MaterialTechnicalValueName ?? string.Empty).ToLowerInvariant().Contains(search) ||
                        x.Quantity.ToString().Contains(search) ||
                        (x.Reason ?? string.Empty).ToLowerInvariant().Contains(search) ||
                        (x.Comment ?? string.Empty).ToLowerInvariant().Contains(search) ||
                        x.MoveDate.ToString("dd.MM.yyyy HH:mm").Contains(search) ||
                        x.MoveDate.ToString("dd.MM.yyyy").Contains(search))
                    .ToList();
            }

            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : pageSize;

            var totalCount = items.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var pagedItems = items
                .OrderByDescending(x => x.MoveDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SearchText = searchText ?? string.Empty;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;

            return View(pagedItems);
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