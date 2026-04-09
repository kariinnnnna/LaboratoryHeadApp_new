using Microsoft.AspNetCore.Mvc;
using ScheduleServiceContracts.BindingModels;

namespace LaboratoryHeadApp.Controllers
{
    public class DutyPersonController : Controller
    {
        private readonly IScheduleApiClient _scheduleApiClient;

        public DutyPersonController(IScheduleApiClient scheduleApiClient)
        {
            _scheduleApiClient = scheduleApiClient;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _scheduleApiClient.GetDutyPersonsAsync() ?? new();
            return View(model);
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