using Microsoft.AspNetCore.Mvc;
using MolServiceContracts.BindingModels;
using MolServiceContracts.ViewModels;
using MOLServiceWebClient;

namespace LaboratoryHeadApp.Controllers
{
    public class SoftwareController : Controller
    {
        private readonly IMolApiClient _client;

        public SoftwareController(IMolApiClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchText,
            int page = 1,
            int pageSize = 20)
        {
            var result = await _client.GetSoftwaresAsync()
                ?? new List<SoftwareViewModel>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var search = searchText.Trim().ToLowerInvariant();

                result = result
                    .Where(x =>
                        (x.SoftwareName ?? string.Empty).ToLowerInvariant().Contains(search) ||
                        (x.SoftwareDescription ?? string.Empty).ToLowerInvariant().Contains(search) ||
                        (x.SoftwareKey ?? string.Empty).ToLowerInvariant().Contains(search) ||
                        (x.SoftwareK ?? string.Empty).ToLowerInvariant().Contains(search))
                    .ToList();
            }

            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : pageSize;

            var totalCount = result.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var pagedResult = result
                .OrderBy(x => x.SoftwareName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SearchText = searchText ?? string.Empty;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;

            return View(pagedResult);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new SoftwareBindingModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(SoftwareBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _client.CreateSoftwareAsync(model);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Не удалось создать программное обеспечение");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var element = await _client.GetSoftwareAsync(id);

            if (element == null)
            {
                return NotFound();
            }

            var model = new SoftwareBindingModel
            {
                Id = element.Id,
                SoftwareName = element.SoftwareName,
                SoftwareDescription = element.SoftwareDescription,
                SoftwareKey = element.SoftwareKey,
                SoftwareK = element.SoftwareK
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SoftwareBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _client.UpdateSoftwareAsync(model);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Не удалось обновить программное обеспечение");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _client.DeleteSoftwareAsync(id);

                if (!result)
                {
                    TempData["ErrorMessage"] = "Не удалось удалить программное обеспечение";
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