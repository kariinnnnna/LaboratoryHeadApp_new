using Microsoft.AspNetCore.Mvc;
using ScheduleServiceContracts.BindingModels;
using ScheduleServiceContracts.ViewModels;

namespace LaboratoryHeadApp.Controllers
{
    public class DutyPersonController : Controller
    {
        private readonly IScheduleApiClient _scheduleApiClient;

        public DutyPersonController(IScheduleApiClient scheduleApiClient)
        {
            _scheduleApiClient = scheduleApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchText,
            int page = 1,
            int pageSize = 20)
        {
            var model = await _scheduleApiClient.GetDutyPersonsAsync()
                ?? new List<DutyPersonViewModel>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var search = searchText.Trim().ToLowerInvariant();

                model = model
                    .Where(x =>
                        (x.LastName ?? string.Empty).ToLowerInvariant().Contains(search) ||
                        (x.FirstName ?? string.Empty).ToLowerInvariant().Contains(search) ||
                        (x.Position ?? string.Empty).ToLowerInvariant().Contains(search) ||
                        (x.Phone ?? string.Empty).ToLowerInvariant().Contains(search) ||
                        (x.Email ?? string.Empty).ToLowerInvariant().Contains(search) ||
                        (x.FullName ?? string.Empty).ToLowerInvariant().Contains(search))
                    .ToList();
            }

            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : pageSize;

            var totalCount = model.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var pagedModel = model
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SearchText = searchText ?? string.Empty;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;

            return View(pagedModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new DutyPersonBindingModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(DutyPersonBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _scheduleApiClient.CreateDutyPersonAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dutyPerson = await _scheduleApiClient.GetDutyPersonByIdAsync(id);

            if (dutyPerson == null)
            {
                return NotFound();
            }

            var model = new DutyPersonBindingModel
            {
                Id = dutyPerson.Id,
                LastName = dutyPerson.LastName,
                FirstName = dutyPerson.FirstName,
                Position = dutyPerson.Position,
                Phone = dutyPerson.Phone,
                Email = dutyPerson.Email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DutyPersonBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _scheduleApiClient.UpdateDutyPersonAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _scheduleApiClient.DeleteDutyPersonAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}