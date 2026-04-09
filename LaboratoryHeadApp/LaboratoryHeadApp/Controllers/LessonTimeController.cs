using Microsoft.AspNetCore.Mvc;
using ScheduleServiceContracts.BindingModels;

namespace LaboratoryHeadApp.Controllers
{
    public class LessonTimeController : Controller
    {
        private readonly IScheduleApiClient _scheduleApiClient;

        public LessonTimeController(IScheduleApiClient scheduleApiClient)
        {
            _scheduleApiClient = scheduleApiClient;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _scheduleApiClient.GetLessonTimesAsync() ?? new();
            return View(model.OrderBy(x => x.PairNumber).ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new LessonTimeBindingModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(LessonTimeBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _scheduleApiClient.CreateLessonTimeAsync(model);
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
            var lessonTime = await _scheduleApiClient.GetLessonTimeByIdAsync(id);
            if (lessonTime == null)
            {
                return NotFound();
            }

            var model = new LessonTimeBindingModel
            {
                Id = lessonTime.Id,
                PairNumber = lessonTime.PairNumber,
                StartTime = lessonTime.StartTime,
                EndTime = lessonTime.EndTime,
                Description = lessonTime.Description
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(LessonTimeBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _scheduleApiClient.UpdateLessonTimeAsync(model);
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
                await _scheduleApiClient.DeleteLessonTimeAsync(id);
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