using LaboratoryHeadApp.Helpers;
using LaboratoryHeadApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ScheduleServiceContracts.BindingModels;
using ScheduleServiceContracts.ViewModels;

namespace LaboratoryHeadApp.Controllers
{
    public class DutyScheduleController : Controller
    {
        private readonly IScheduleApiClient _scheduleApiClient;

        public DutyScheduleController(IScheduleApiClient scheduleApiClient)
        {
            _scheduleApiClient = scheduleApiClient;
        }

        public async Task<IActionResult> Index()
        {
            var schedules = await _scheduleApiClient.GetDutyScheduleAsync()
                ?? new List<DutyScheduleViewModel>();

            var lessonTimes = (await _scheduleApiClient.GetLessonTimesAsync()
                    ?? new List<LessonTimeViewModel>())
                .OrderBy(x => x.PairNumber)
                .ToList();

            var today = DateTime.Today;
            var currentWeekMonday = GetMonday(today);
            var nextWeekMonday = currentWeekMonday.AddDays(7);

            var currentWeekDates = Enumerable.Range(0, 7)
                .Select(i => currentWeekMonday.AddDays(i))
                .ToList();

            var nextWeekDates = Enumerable.Range(0, 7)
                .Select(i => nextWeekMonday.AddDays(i))
                .ToList();

            var model = new DutyScheduleIndexViewModel
            {
                LessonTimes = lessonTimes,
                CurrentWeekDates = currentWeekDates,
                NextWeekDates = nextWeekDates,
                CurrentWeekData = BuildWeekData(currentWeekDates, lessonTimes, schedules),
                NextWeekData = BuildWeekData(nextWeekDates, lessonTimes, schedules)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var lessonTimes = (await _scheduleApiClient.GetLessonTimesAsync()
                    ?? new List<LessonTimeViewModel>())
                .OrderBy(x => x.PairNumber)
                .ToList();

            var dutyPersons = (await _scheduleApiClient.GetDutyPersonsAsync()
                    ?? new List<DutyPersonViewModel>())
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .ToList();

            var today = DateTime.Today;
            var currentWeekMonday = GetMonday(today);

            var dates = Enumerable.Range(0, 14)
                .Select(i => currentWeekMonday.AddDays(i))
                .ToList();

            var cells = new List<DutyScheduleCellEditViewModel>();

            foreach (var date in dates)
            {
                foreach (var lessonTime in lessonTimes)
                {
                    cells.Add(new DutyScheduleCellEditViewModel
                    {
                        Date = date,
                        LessonTimeId = lessonTime.Id,
                        PairNumber = lessonTime.PairNumber,
                        StartTime = lessonTime.StartTime,
                        EndTime = lessonTime.EndTime
                    });
                }
            }

            var model = new DutyScheduleCreateViewModel
            {
                Cells = cells,
                LessonTimes = lessonTimes,
                DutyPersons = dutyPersons,
                DutyPersonItems = BuildDutyPersonItems(dutyPersons)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DutyScheduleCreateViewModel model)
        {
            await FillDutyScheduleModel(model);

            foreach (var cell in model.Cells ?? new List<DutyScheduleCellEditViewModel>())
            {
                if (cell.DutyPerson1Id.HasValue &&
                    cell.DutyPerson2Id.HasValue &&
                    cell.DutyPerson1Id.Value > 0 &&
                    cell.DutyPerson2Id.Value > 0 &&
                    cell.DutyPerson1Id.Value == cell.DutyPerson2Id.Value)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Нельзя выбрать одного и того же дежурного дважды: {cell.Date:dd.MM.yyyy}, пара {cell.PairNumber}");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                foreach (var cell in model.Cells ?? new List<DutyScheduleCellEditViewModel>())
                {
                    if (cell.LessonTimeId <= 0)
                    {
                        continue;
                    }

                    if (cell.DutyPerson1Id.HasValue && cell.DutyPerson1Id.Value > 0)
                    {
                        await _scheduleApiClient.CreateDutyScheduleAsync(new DutyScheduleBindingModel
                        {
                            Date = cell.Date,
                            LessonTimeId = cell.LessonTimeId,
                            DutyPersonId = cell.DutyPerson1Id.Value
                        });
                    }

                    if (cell.DutyPerson2Id.HasValue && cell.DutyPerson2Id.Value > 0)
                    {
                        await _scheduleApiClient.CreateDutyScheduleAsync(new DutyScheduleBindingModel
                        {
                            Date = cell.Date,
                            LessonTimeId = cell.LessonTimeId,
                            DutyPersonId = cell.DutyPerson2Id.Value
                        });
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.ToString());
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var lessonTimes = (await _scheduleApiClient.GetLessonTimesAsync()
                    ?? new List<LessonTimeViewModel>())
                .OrderBy(x => x.PairNumber)
                .ToList();

            var dutyPersons = (await _scheduleApiClient.GetDutyPersonsAsync()
                    ?? new List<DutyPersonViewModel>())
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .ToList();

            var schedules = await _scheduleApiClient.GetDutyScheduleAsync()
                ?? new List<DutyScheduleViewModel>();

            var today = DateTime.Today;
            var currentWeekMonday = GetMonday(today);

            var dates = Enumerable.Range(0, 14)
                .Select(i => currentWeekMonday.AddDays(i))
                .ToList();

            var cells = new List<DutyScheduleCellEditViewModel>();

            foreach (var date in dates)
            {
                foreach (var lessonTime in lessonTimes)
                {
                    var sameCellSchedules = schedules
                        .Where(x => x.Date.Date == date.Date &&
                                    x.LessonTimeId == lessonTime.Id)
                        .OrderBy(x => x.Id)
                        .Take(2)
                        .ToList();

                    cells.Add(new DutyScheduleCellEditViewModel
                    {
                        Date = date,
                        LessonTimeId = lessonTime.Id,
                        PairNumber = lessonTime.PairNumber,
                        StartTime = lessonTime.StartTime,
                        EndTime = lessonTime.EndTime,
                        DutyPerson1Id = sameCellSchedules.ElementAtOrDefault(0)?.DutyPersonId,
                        DutyPerson2Id = sameCellSchedules.ElementAtOrDefault(1)?.DutyPersonId
                    });
                }
            }

            var model = new DutyScheduleCreateViewModel
            {
                Cells = cells,
                LessonTimes = lessonTimes,
                DutyPersons = dutyPersons,
                DutyPersonItems = BuildDutyPersonItems(dutyPersons)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DutyScheduleCreateViewModel model)
        {
            await FillDutyScheduleModel(model);

            foreach (var cell in model.Cells ?? new List<DutyScheduleCellEditViewModel>())
            {
                if (cell.DutyPerson1Id.HasValue &&
                    cell.DutyPerson2Id.HasValue &&
                    cell.DutyPerson1Id.Value > 0 &&
                    cell.DutyPerson2Id.Value > 0 &&
                    cell.DutyPerson1Id.Value == cell.DutyPerson2Id.Value)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Нельзя выбрать одного и того же дежурного дважды: {cell.Date:dd.MM.yyyy}, пара {cell.PairNumber}");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var allSchedules = await _scheduleApiClient.GetDutyScheduleAsync()
                    ?? new List<DutyScheduleViewModel>();

                foreach (var cell in model.Cells ?? new List<DutyScheduleCellEditViewModel>())
                {
                    if (cell.LessonTimeId <= 0)
                    {
                        continue;
                    }

                    var existingSchedules = allSchedules
                        .Where(x => x.Date.Date == cell.Date.Date &&
                                    x.LessonTimeId == cell.LessonTimeId)
                        .OrderBy(x => x.Id)
                        .ToList();

                    await UpsertOrDeleteDutyScheduleAsync(
                        existingSchedules.ElementAtOrDefault(0),
                        cell.DutyPerson1Id,
                        cell.Date,
                        cell.LessonTimeId);

                    await UpsertOrDeleteDutyScheduleAsync(
                        existingSchedules.ElementAtOrDefault(1),
                        cell.DutyPerson2Id,
                        cell.Date,
                        cell.LessonTimeId);

                    foreach (var extraSchedule in existingSchedules.Skip(2))
                    {
                        await _scheduleApiClient.DeleteDutyScheduleAsync(extraSchedule.Id);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.ToString());
                return View(model);
            }
        }

        private async Task FillDutyScheduleModel(DutyScheduleCreateViewModel model)
        {
            var lessonTimes = (await _scheduleApiClient.GetLessonTimesAsync()
                    ?? new List<LessonTimeViewModel>())
                .OrderBy(x => x.PairNumber)
                .ToList();

            var dutyPersons = (await _scheduleApiClient.GetDutyPersonsAsync()
                    ?? new List<DutyPersonViewModel>())
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .ToList();

            model.LessonTimes = lessonTimes;
            model.DutyPersons = dutyPersons;
            model.DutyPersonItems = BuildDutyPersonItems(dutyPersons);
        }

        private static List<SelectListItem> BuildDutyPersonItems(
            IEnumerable<DutyPersonViewModel> dutyPersons)
        {
            return dutyPersons
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = $"{DutyPersonNameHelper.GetInitials(x.FullName)} ({x.FullName})"
                })
                .ToList();
        }

        private async Task UpsertOrDeleteDutyScheduleAsync(
            DutyScheduleViewModel? existingSchedule,
            int? dutyPersonId,
            DateTime date,
            int lessonTimeId)
        {
            if (dutyPersonId.HasValue && dutyPersonId.Value > 0)
            {
                var bindingModel = new DutyScheduleBindingModel
                {
                    Id = existingSchedule?.Id ?? 0,
                    Date = date,
                    LessonTimeId = lessonTimeId,
                    DutyPersonId = dutyPersonId.Value
                };

                if (existingSchedule == null)
                {
                    await _scheduleApiClient.CreateDutyScheduleAsync(bindingModel);
                }
                else
                {
                    await _scheduleApiClient.UpdateDutyScheduleAsync(bindingModel);
                }

                return;
            }

            if (existingSchedule != null)
            {
                await _scheduleApiClient.DeleteDutyScheduleAsync(existingSchedule.Id);
            }
        }

        private static DateTime GetMonday(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private static Dictionary<string, List<string>> BuildWeekData(
            List<DateTime> weekDates,
            List<LessonTimeViewModel> lessonTimes,
            List<DutyScheduleViewModel> schedules)
        {
            var result = new Dictionary<string, List<string>>();

            foreach (var date in weekDates)
            {
                foreach (var lessonTime in lessonTimes)
                {
                    var key = GetCellKey(date, lessonTime.Id);

                    var initials = schedules
                        .Where(x => x.Date.Date == date.Date &&
                                    x.LessonTimeId == lessonTime.Id)
                        .OrderBy(x => x.Id)
                        .Select(x => DutyPersonNameHelper.GetInitials(x.DutyPersonName))
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct()
                        .ToList();

                    result[key] = initials;
                }
            }

            return result;
        }

        public static string GetCellKey(DateTime date, int lessonTimeId)
        {
            return $"{date:yyyy-MM-dd}_{lessonTimeId}";
        }
    }
}