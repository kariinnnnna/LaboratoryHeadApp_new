using LaboratoryHeadApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MolServiceContracts.ViewModels;
using MOLServiceWebClient;
using ScheduleServiceContracts.BindingModels;
using ScheduleServiceContracts.ViewModels;
using ScheduleServiceDataModels.Enums;

namespace LaboratoryHeadApp.Controllers
{
    public class ClassroomReservationController : Controller
    {
        private readonly IScheduleApiClient _scheduleApiClient;
        private readonly IMolApiClient _molApiClient;

        public ClassroomReservationController(
            IScheduleApiClient scheduleApiClient,
            IMolApiClient molApiClient)
        {
            _scheduleApiClient = scheduleApiClient;
            _molApiClient = molApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Create(DateTime? date, string? classroomNumber)
        {
            var model = new ClassroomReservationCreateViewModel
            {
                Date = date ?? DateTime.Today,
                ClassroomNumber = classroomNumber ?? string.Empty
            };

            await FillDictionaries(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassroomReservationCreateViewModel model)
        {
            await FillDictionaries(model);

            ApplySelectedDictionaryValues(model);

            if (string.IsNullOrWhiteSpace(model.ClassroomNumber))
            {
                ModelState.AddModelError(nameof(model.ClassroomNumber), "Укажите аудиторию или выберите ее из списка.");
            }

            if (string.IsNullOrWhiteSpace(model.TeacherName))
            {
                ModelState.AddModelError(nameof(model.TeacherName), "Укажите преподавателя или выберите его из списка.");
            }

            if (string.IsNullOrWhiteSpace(model.GroupName))
            {
                ModelState.AddModelError(nameof(model.GroupName), "Укажите группу или выберите ее из списка.");
            }

            if (!model.LessonTimeId.HasValue && (!model.StartTime.HasValue || !model.EndTime.HasValue))
            {
                ModelState.AddModelError("", "Укажите либо пару, либо время начала и окончания.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var binding = new ScheduleItemBindingModel
                {
                    Type = ScheduleItemType.Consultation,
                    Date = DateTime.SpecifyKind(model.Date.Date, DateTimeKind.Utc),
                    LessonTimeId = model.LessonTimeId,
                    StartTime = model.LessonTimeId.HasValue ? null : model.StartTime,
                    EndTime = model.LessonTimeId.HasValue ? null : model.EndTime,

                    ClassroomId = model.ClassroomId,
                    ClassroomNumber = model.ClassroomNumber.Trim(),

                    TeacherId = model.TeacherId,
                    TeacherName = model.TeacherName.Trim(),

                    GroupId = model.GroupId,
                    GroupName = model.GroupName.Trim(),

                    Subject = model.Subject,
                    Comment = model.Comment,
                    IsImported = false
                };

                await _scheduleApiClient.CreateScheduleItemAsync(binding);

                TempData["SuccessMessage"] = "Бронирование аудитории успешно создано.";

                return RedirectToAction("LessonsRooms", "Schedule", new
                {
                    date = model.Date.ToString("yyyy-MM-dd")
                });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        private async Task FillDictionaries(ClassroomReservationCreateViewModel model)
        {
            var lessonTimes = await _scheduleApiClient.GetLessonTimesAsync() ?? new List<LessonTimeViewModel>();
            var teachers = await _scheduleApiClient.GetTeachersAsync() ?? new List<TeacherViewModel>();
            var groups = await _scheduleApiClient.GetGroupsAsync() ?? new List<GroupViewModel>();
            var classrooms = await _molApiClient.GetClassroomsAsync() ?? new List<ClassroomViewModel>();

            model.LessonTimes = lessonTimes
                .OrderBy(x => x.PairNumber)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = $"{x.PairNumber} пара ({x.StartTime:hh\\:mm}-{x.EndTime:hh\\:mm})"
                })
                .ToList();

            model.Teachers = teachers
                .Where(x => !string.IsNullOrWhiteSpace(x.TeacherName))
                .OrderBy(x => x.TeacherName)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.TeacherName!
                })
                .ToList();

            model.Groups = groups
                .Where(x => !string.IsNullOrWhiteSpace(x.GroupName))
                .OrderBy(x => x.GroupName)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.GroupName!
                })
                .ToList();

            model.Classrooms = classrooms
                .Where(x => !string.IsNullOrWhiteSpace(x.Number))
                .OrderBy(x => x.Number)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Number!
                })
                .ToList();
        }

        private void ApplySelectedDictionaryValues(ClassroomReservationCreateViewModel model)
        {
            if (model.SelectedTeacherId.HasValue)
            {
                var selectedTeacher = model.Teachers.FirstOrDefault(x => x.Value == model.SelectedTeacherId.Value.ToString());
                if (selectedTeacher != null)
                {
                    model.TeacherId = model.SelectedTeacherId;
                    model.TeacherName = selectedTeacher.Text;
                }
            }

            if (model.SelectedGroupId.HasValue)
            {
                var selectedGroup = model.Groups.FirstOrDefault(x => x.Value == model.SelectedGroupId.Value.ToString());
                if (selectedGroup != null)
                {
                    model.GroupId = model.SelectedGroupId;
                    model.GroupName = selectedGroup.Text;
                }
            }

            if (model.SelectedClassroomId.HasValue)
            {
                var selectedClassroom = model.Classrooms.FirstOrDefault(x => x.Value == model.SelectedClassroomId.Value.ToString());
                if (selectedClassroom != null)
                {
                    model.ClassroomId = model.SelectedClassroomId;
                    model.ClassroomNumber = selectedClassroom.Text;
                }
            }
        }
    }
}
