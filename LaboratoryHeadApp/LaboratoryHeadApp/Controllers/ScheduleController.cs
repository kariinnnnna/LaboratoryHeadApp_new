using LaboratoryHeadApp.Models;
using Microsoft.AspNetCore.Mvc;
using MolServiceContracts.ViewModels;
using MOLServiceWebClient;
using ScheduleServiceContracts.BindingModels;
using ScheduleServiceContracts.ViewModels;

namespace LaboratoryHeadApp.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly IScheduleApiClient _scheduleApiClient;
        private readonly IMolApiClient _molApiClient;
        private readonly IConfiguration _configuration;

        public ScheduleController(
            IScheduleApiClient scheduleApiClient,
            IMolApiClient molApiClient,
            IConfiguration configuration)
        {
            _scheduleApiClient = scheduleApiClient;
            _molApiClient = molApiClient;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> LessonsRooms(DateTime? date)
        {
            var selectedDate = (date ?? DateTime.Today).Date;

            var scheduleItems = await _scheduleApiClient.GetScheduleAsync()
                ?? new List<ScheduleItemViewModel>();

            var classrooms = await _molApiClient.GetClassroomsAsync()
                ?? new List<ClassroomViewModel>();

            var teachers = await _scheduleApiClient.GetTeachersAsync()
                ?? new List<TeacherViewModel>();

            var groups = await _scheduleApiClient.GetGroupsAsync()
                ?? new List<GroupViewModel>();

            var classroomNumbers = classrooms
                .Where(x => !string.IsNullOrWhiteSpace(x.Number))
                .Select(x => x.Number!.Trim())
                .Where(IsTargetClassroom)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var teacherNames = teachers
                .Where(x => !string.IsNullOrWhiteSpace(x.TeacherName))
                .Select(x => x.TeacherName!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var groupNames = groups
                .Where(x => !string.IsNullOrWhiteSpace(x.GroupName))
                .Select(x => x.GroupName!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var dayItems = scheduleItems
                .Where(x => x.Date.Date == selectedDate)
                .Where(x => !string.IsNullOrWhiteSpace(x.ClassroomNumber))
                .Where(x => classroomNumbers.Contains(x.ClassroomNumber!.Trim(), StringComparer.OrdinalIgnoreCase))
                .ToList();

            var model = new LessonsRoomsPageViewModel
            {
                SelectedDate = selectedDate.ToString("yyyy-MM-dd"),
                Classrooms = classroomNumbers,
                Teachers = teacherNames,
                Groups = groupNames,
                Lessons = dayItems
                    .Select(x => new LessonsRoomsItemViewModel
                    {
                        ClassroomNumber = x.ClassroomNumber ?? string.Empty,
                        PairNumber = x.PairNumber,
                        TypeName = x.TypeName,
                        Subject = x.Subject,
                        TeacherName = x.TeacherName ?? string.Empty,
                        GroupName = x.GroupName ?? string.Empty,
                        Subgroup = x.Comment ?? string.Empty,
                        IsImported = x.IsImported
                    })
                    .OrderBy(x => x.ClassroomNumber)
                    .ThenBy(x => x.PairNumber)
                    .ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> LessonsGroups(DateTime? date)
        {
            var selectedDate = (date ?? DateTime.Today).Date;

            var items = await _scheduleApiClient.GetScheduleAsync()
                ?? new List<ScheduleItemViewModel>();

            var teachers = await _scheduleApiClient.GetTeachersAsync()
                ?? new List<TeacherViewModel>();

            var groups = await _scheduleApiClient.GetGroupsAsync()
                ?? new List<GroupViewModel>();

            var classrooms = await _molApiClient.GetClassroomsAsync()
                ?? new List<ClassroomViewModel>();

            var dayItems = items
                .Where(x => x.Date.Date == selectedDate)
                .ToList();

            var model = new LessonsGroupsPageViewModel
            {
                SelectedDate = selectedDate.ToString("yyyy-MM-dd"),
                Groups = groups
                    .Where(x => !string.IsNullOrWhiteSpace(x.GroupName))
                    .Select(x => x.GroupName!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList(),
                Teachers = teachers
                    .Where(x => !string.IsNullOrWhiteSpace(x.TeacherName))
                    .Select(x => x.TeacherName!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList(),
                Classrooms = classrooms
                    .Where(x => !string.IsNullOrWhiteSpace(x.Number))
                    .Select(x => x.Number!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList(),
                Lessons = dayItems
                    .Select(x => new LessonsGroupsItemViewModel
                    {
                        GroupName = x.GroupName ?? string.Empty,
                        PairNumber = x.PairNumber,
                        TypeName = x.TypeName,
                        Subject = x.Subject,
                        ClassroomNumber = x.ClassroomNumber ?? string.Empty,
                        TeacherName = x.TeacherName ?? string.Empty,
                        StartTime = x.StartTime?.ToString(@"hh\:mm") ?? string.Empty,
                        EndTime = x.EndTime?.ToString(@"hh\:mm") ?? string.Empty,
                        Subgroup = x.Comment ?? string.Empty,
                        IsImported = x.IsImported
                    })
                    .OrderBy(x => x.GroupName)
                    .ThenBy(x => x.PairNumber)
                    .ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> LessonsTeachers(DateTime? date)
        {
            var selectedDate = (date ?? DateTime.Today).Date;

            var items = await _scheduleApiClient.GetScheduleAsync()
                ?? new List<ScheduleItemViewModel>();

            var teachers = await _scheduleApiClient.GetTeachersAsync()
                ?? new List<TeacherViewModel>();

            var groups = await _scheduleApiClient.GetGroupsAsync()
                ?? new List<GroupViewModel>();

            var classrooms = await _molApiClient.GetClassroomsAsync()
                ?? new List<ClassroomViewModel>();

            var dayItems = items
                .Where(x => x.Date.Date == selectedDate)
                .ToList();

            var model = new LessonsTeachersPageViewModel
            {
                SelectedDate = selectedDate.ToString("yyyy-MM-dd"),
                Teachers = teachers
                    .Where(x => !string.IsNullOrWhiteSpace(x.TeacherName))
                    .Select(x => x.TeacherName!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList(),
                Groups = groups
                    .Where(x => !string.IsNullOrWhiteSpace(x.GroupName))
                    .Select(x => x.GroupName!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList(),
                Classrooms = classrooms
                    .Where(x => !string.IsNullOrWhiteSpace(x.Number))
                    .Select(x => x.Number!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList(),
                Lessons = dayItems
                    .Select(x => new LessonsTeachersItemViewModel
                    {
                        TeacherName = x.TeacherName ?? string.Empty,
                        PairNumber = x.PairNumber,
                        TypeName = x.TypeName,
                        Subject = x.Subject,
                        ClassroomNumber = x.ClassroomNumber ?? string.Empty,
                        GroupName = x.GroupName ?? string.Empty,
                        Subgroup = x.Comment ?? string.Empty,
                        IsImported = x.IsImported
                    })
                    .OrderBy(x => x.TeacherName)
                    .ThenBy(x => x.PairNumber)
                    .ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> SyncLessons(string? returnAction, DateTime? date)
        {
            try
            {
                var folderPath = _configuration["ApiSettings:ScheduleImportFolderPath"];

                if (string.IsNullOrWhiteSpace(folderPath))
                {
                    TempData["ErrorMessage"] = "Не указан путь к папке для синхронизации расписания.";
                    return RedirectToAction(returnAction ?? nameof(LessonsGroups), new { date });
                }

                await _scheduleApiClient.ImportGroupSchedulesFromFolderAsync(
                    new UniversityScheduleImportFolderBindingModel
                    {
                        FolderPath = folderPath
                    });

                TempData["SuccessMessage"] = "Синхронизация расписания завершена.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(returnAction ?? nameof(LessonsGroups), new { date });
        }

        private static bool IsTargetClassroom(string classroom)
        {
            var value = classroom.Trim().Replace('–', '-');

            return System.Text.RegularExpressions.Regex.IsMatch(
                value,
                @"^(3[-_]4.+)$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        public IActionResult Duty() => View();
    }
}