using LaboratoryHeadApp.Controllers;
using LaboratoryHeadApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MolServiceContracts.ViewModels;
using MOLServiceWebClient;
using Moq;
using ScheduleServiceContracts.BindingModels;
using ScheduleServiceContracts.ViewModels;
using ScheduleServiceDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaboratoryHeadTest.Controllers
{
    public class ClassroomReservationControllerTests
    {
        [Fact]
        public async Task Create_Get_ShouldReturnView_WithPrefilledDateAndClassroom_AndFilledDictionaries()
        {
            var scheduleApiClientMock = new Mock<IScheduleApiClient>();
            var molApiClientMock = new Mock<IMolApiClient>();

            scheduleApiClientMock
                .Setup(x => x.GetLessonTimesAsync())
                .ReturnsAsync(new List<LessonTimeViewModel>
                {
                    new LessonTimeViewModel
                    {
                        Id = 1,
                        PairNumber = 2,
                        StartTime = new TimeSpan(10, 15, 0),
                        EndTime = new TimeSpan(11, 45, 0)
                    }
                });

            scheduleApiClientMock
                .Setup(x => x.GetTeachersAsync())
                .ReturnsAsync(new List<TeacherViewModel>
                {
                    new TeacherViewModel { Id = 5, TeacherName = "Иванов И.И." }
                });

            scheduleApiClientMock
                .Setup(x => x.GetGroupsAsync())
                .ReturnsAsync(new List<GroupViewModel>
                {
                    new GroupViewModel { Id = 7, GroupName = "ИВТ-301" }
                });

            molApiClientMock
                .Setup(x => x.GetClassroomsAsync())
                .ReturnsAsync(new List<ClassroomViewModel>
                {
                    new ClassroomViewModel { Id = 9, Number = "405" }
                });

            var controller = CreateController(scheduleApiClientMock, molApiClientMock);

            var date = new DateTime(2026, 4, 25);

            var result = await controller.Create(date, "405") as ViewResult;

            Assert.NotNull(result);

            var model = Assert.IsType<ClassroomReservationCreateViewModel>(result!.Model);
            Assert.Equal(date, model.Date);
            Assert.Equal("405", model.ClassroomNumber);

            Assert.Single(model.LessonTimes);
            Assert.Single(model.Teachers);
            Assert.Single(model.Groups);
            Assert.Single(model.Classrooms);

            Assert.Equal("5", model.Teachers[0].Value);
            Assert.Equal("Иванов И.И.", model.Teachers[0].Text);
        }

        [Fact]
        public async Task Create_Post_ShouldReturnView_WhenRequiredFieldsAreMissing()
        {
            var scheduleApiClientMock = new Mock<IScheduleApiClient>();
            var molApiClientMock = new Mock<IMolApiClient>();

            SetupDictionaries(scheduleApiClientMock, molApiClientMock);

            var controller = CreateController(scheduleApiClientMock, molApiClientMock);

            var model = new ClassroomReservationCreateViewModel
            {
                Date = new DateTime(2026, 4, 25),
                Subject = "Консультация",
                ClassroomNumber = "",
                TeacherName = "",
                GroupName = "",
                LessonTimeId = null,
                StartTime = null,
                EndTime = null
            };

            var result = await controller.Create(model) as ViewResult;

            Assert.NotNull(result);
            var returnedModel = Assert.IsType<ClassroomReservationCreateViewModel>(result!.Model);

            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(nameof(model.ClassroomNumber)));
            Assert.True(controller.ModelState.ContainsKey(nameof(model.TeacherName)));
            Assert.True(controller.ModelState.ContainsKey(nameof(model.GroupName)));
            Assert.True(controller.ModelState.ContainsKey(""));

            scheduleApiClientMock.Verify(x => x.CreateScheduleItemAsync(It.IsAny<ScheduleItemBindingModel>()), Times.Never);
            Assert.Equal(model, returnedModel);
        }

        [Fact]
        public async Task Create_Post_ShouldUseSelectedDictionaryValues_TrimFields_CreateReservation_AndRedirect()
        {
            var scheduleApiClientMock = new Mock<IScheduleApiClient>();
            var molApiClientMock = new Mock<IMolApiClient>();

            scheduleApiClientMock
                .Setup(x => x.GetLessonTimesAsync())
                .ReturnsAsync(new List<LessonTimeViewModel>
                {
                    new LessonTimeViewModel
                    {
                        Id = 3,
                        PairNumber = 3,
                        StartTime = new TimeSpan(12, 0, 0),
                        EndTime = new TimeSpan(13, 30, 0)
                    }
                });

            scheduleApiClientMock
                .Setup(x => x.GetTeachersAsync())
                .ReturnsAsync(new List<TeacherViewModel>
                {
                    new TeacherViewModel { Id = 15, TeacherName = "Петров П.П." }
                });

            scheduleApiClientMock
                .Setup(x => x.GetGroupsAsync())
                .ReturnsAsync(new List<GroupViewModel>
                {
                    new GroupViewModel { Id = 20, GroupName = "ПМИ-401" }
                });

            molApiClientMock
                .Setup(x => x.GetClassroomsAsync())
                .ReturnsAsync(new List<ClassroomViewModel>
                {
                    new ClassroomViewModel { Id = 30, Number = "501" }
                });

            scheduleApiClientMock
                .Setup(x => x.CreateScheduleItemAsync(It.Is<ScheduleItemBindingModel>(m =>
                    m.Type == ScheduleItemType.Consultation &&
                    m.Date == DateTime.SpecifyKind(new DateTime(2026, 4, 26), DateTimeKind.Utc) &&
                    m.LessonTimeId == 3 &&
                    m.StartTime == null &&
                    m.EndTime == null &&
                    m.ClassroomId == 30 &&
                    m.ClassroomNumber == "501" &&
                    m.TeacherId == 15 &&
                    m.TeacherName == "Петров П.П." &&
                    m.GroupId == 20 &&
                    m.GroupName == "ПМИ-401" &&
                    m.Subject == "Консультация по БД" &&
                    m.Comment == "Подготовка к зачету" &&
                    m.IsImported == false)))
                .ReturnsAsync(new ScheduleItemViewModel
                {
                    Id = 1,
                    Subject = "Консультация по БД"
                });

            var controller = CreateController(scheduleApiClientMock, molApiClientMock);

            var model = new ClassroomReservationCreateViewModel
            {
                Date = new DateTime(2026, 4, 26),
                Subject = "Консультация по БД",
                Comment = "Подготовка к зачету",
                LessonTimeId = 3,

                SelectedTeacherId = 15,
                SelectedGroupId = 20,
                SelectedClassroomId = 30,

                TeacherName = "  не то значение  ",
                GroupName = "  не то значение  ",
                ClassroomNumber = "  не то значение  "
            };

            var result = await controller.Create(model) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("LessonsRooms", result!.ActionName);
            Assert.Equal("Schedule", result.ControllerName);
            Assert.Equal("Бронирование аудитории успешно создано.", controller.TempData["SuccessMessage"]);

            scheduleApiClientMock.Verify(x => x.CreateScheduleItemAsync(It.IsAny<ScheduleItemBindingModel>()), Times.Once);
        }

        [Fact]
        public async Task Create_Post_ShouldReturnView_AndAddModelError_WhenApiThrows()
        {
            var scheduleApiClientMock = new Mock<IScheduleApiClient>();
            var molApiClientMock = new Mock<IMolApiClient>();

            scheduleApiClientMock
                .Setup(x => x.GetLessonTimesAsync())
                .ReturnsAsync(new List<LessonTimeViewModel>
                {
                    new LessonTimeViewModel
                    {
                        Id = 2,
                        PairNumber = 2,
                        StartTime = new TimeSpan(10, 15, 0),
                        EndTime = new TimeSpan(11, 45, 0)
                    }
                });

            scheduleApiClientMock
                .Setup(x => x.GetTeachersAsync())
                .ReturnsAsync(new List<TeacherViewModel>
                {
                    new TeacherViewModel { Id = 11, TeacherName = "Иванов И.И." }
                });

            scheduleApiClientMock
                .Setup(x => x.GetGroupsAsync())
                .ReturnsAsync(new List<GroupViewModel>
                {
                    new GroupViewModel { Id = 12, GroupName = "ИВТ-302" }
                });

            molApiClientMock
                .Setup(x => x.GetClassroomsAsync())
                .ReturnsAsync(new List<ClassroomViewModel>
                {
                    new ClassroomViewModel { Id = 13, Number = "402" }
                });

            scheduleApiClientMock
                .Setup(x => x.CreateScheduleItemAsync(It.IsAny<ScheduleItemBindingModel>()))
                .ThrowsAsync(new Exception("Аудитория уже занята"));

            var controller = CreateController(scheduleApiClientMock, molApiClientMock);

            var model = new ClassroomReservationCreateViewModel
            {
                Date = new DateTime(2026, 4, 27),
                Subject = "Допзанятие",
                LessonTimeId = 2,
                SelectedTeacherId = 11,
                SelectedGroupId = 12,
                SelectedClassroomId = 13
            };

            var result = await controller.Create(model) as ViewResult;

            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState[string.Empty].Errors, e => e.ErrorMessage == "Аудитория уже занята");

            scheduleApiClientMock.Verify(x => x.CreateScheduleItemAsync(It.IsAny<ScheduleItemBindingModel>()), Times.Once);
        }

        private static ClassroomReservationController CreateController(
            Mock<IScheduleApiClient> scheduleApiClientMock,
            Mock<IMolApiClient> molApiClientMock)
        {
            var controller = new ClassroomReservationController(
                scheduleApiClientMock.Object,
                molApiClientMock.Object);

            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            return controller;
        }

        private static void SetupDictionaries(
            Mock<IScheduleApiClient> scheduleApiClientMock,
            Mock<IMolApiClient> molApiClientMock)
        {
            scheduleApiClientMock
                .Setup(x => x.GetLessonTimesAsync())
                .ReturnsAsync(new List<LessonTimeViewModel>());

            scheduleApiClientMock
                .Setup(x => x.GetTeachersAsync())
                .ReturnsAsync(new List<TeacherViewModel>());

            scheduleApiClientMock
                .Setup(x => x.GetGroupsAsync())
                .ReturnsAsync(new List<GroupViewModel>());

            molApiClientMock
                .Setup(x => x.GetClassroomsAsync())
                .ReturnsAsync(new List<ClassroomViewModel>());
        }
    }
}
