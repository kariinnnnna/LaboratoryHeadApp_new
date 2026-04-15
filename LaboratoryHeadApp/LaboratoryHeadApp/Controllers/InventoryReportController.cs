using LaboratoryHeadApp.Models;
using LaboratoryHeadApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MolServiceContracts.BindingModels;
using MOLServiceWebClient;

namespace LaboratoryHeadApp.Controllers
{
    public class InventoryReportController : Controller
    {
        private readonly IMolApiClient _molApiClient;
        private readonly IInventoryReportPdfService _inventoryReportPdfService;

        public InventoryReportController(IMolApiClient molApiClient, IInventoryReportPdfService inventoryReportPdfService)
        {
            _molApiClient = molApiClient;
            _inventoryReportPdfService = inventoryReportPdfService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> FullInventory()
        {
            try
            {
                var report = await _molApiClient.GetFullInventoryReportAsync();
                return View(report);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> SelectClassrooms()
        {
            try
            {
                var classrooms = await _molApiClient.GetClassroomsAsync() ?? new List<MolServiceContracts.ViewModels.ClassroomViewModel>();

                var model = new InventoryReportClassroomSelectionViewModel
                {
                    Classrooms = classrooms
                        .OrderBy(x => x.Number)
                        .Select(x => new InventoryReportClassroomItemViewModel
                        {
                            ClassroomId = x.Id,
                            ClassroomNumber = x.Number
                        })
                        .ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClassroomsInventory(InventoryReportClassroomSelectionViewModel model)
        {
            try
            {
                var selectedIds = model.Classrooms
                    .Where(x => x.IsSelected)
                    .Select(x => x.ClassroomId)
                    .ToList();

                if (!selectedIds.Any())
                {
                    ModelState.AddModelError(string.Empty, "Выберите хотя бы одну аудиторию.");

                    var classrooms = await _molApiClient.GetClassroomsAsync() ?? new List<MolServiceContracts.ViewModels.ClassroomViewModel>();
                    model.Classrooms = classrooms
                        .OrderBy(x => x.Number)
                        .Select(x => new InventoryReportClassroomItemViewModel
                        {
                            ClassroomId = x.Id,
                            ClassroomNumber = x.Number,
                            IsSelected = model.Classrooms.Any(m => m.ClassroomId == x.Id && m.IsSelected)
                        })
                        .ToList();

                    return View("SelectClassrooms", model);
                }

                var report = await _molApiClient.GetClassroomsInventoryReportAsync(new ClassroomsInventoryReportBindingModel
                {
                    ClassroomIds = selectedIds
                });

                return View(report);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpGet]
        public async Task<IActionResult> DownloadFullInventoryPdf()
        {
            try
            {
                var report = await _molApiClient.GetFullInventoryReportAsync();

                if (report == null)
                {
                    TempData["ErrorMessage"] = "Не удалось получить данные отчёта.";
                    return RedirectToAction(nameof(Index));
                }

                var pdfBytes = _inventoryReportPdfService.GenerateFullInventoryPdf(report);

                var fileName = $"inventory-full-{DateTime.Now:yyyy-MM-dd-HH-mm}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadClassroomsInventoryPdf(ClassroomsInventoryReportBindingModel model)
        {
            try
            {
                if (model == null || model.ClassroomIds == null || model.ClassroomIds.Count == 0)
                {
                    TempData["ErrorMessage"] = "Не выбраны аудитории для формирования PDF.";
                    return RedirectToAction(nameof(SelectClassrooms));
                }

                var report = await _molApiClient.GetClassroomsInventoryReportAsync(model);

                if (report == null)
                {
                    TempData["ErrorMessage"] = "Не удалось получить данные отчёта.";
                    return RedirectToAction(nameof(Index));
                }

                var pdfBytes = _inventoryReportPdfService.GenerateClassroomsInventoryPdf(report);

                var fileName = $"inventory-classrooms-{DateTime.Now:yyyy-MM-dd-HH-mm}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
