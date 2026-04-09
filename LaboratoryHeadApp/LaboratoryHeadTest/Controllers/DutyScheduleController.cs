using LaboratoryHeadApp.Controllers;
using LaboratoryHeadApp.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ScheduleServiceContracts.BindingModels;
using ScheduleServiceContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaboratoryHeadTest.Controllers
{
    public class DutyScheduleControllerTests
    {
        [Fact]
        public async Task Create_Get_ShouldBuildGridFor14DaysAndAllLessonTimes()
        {
            var scheduleApiClientMock = new Mock<IScheduleApiClient>();

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
                    },
                    new LessonTimeViewModel
                    {
                        Id = 1,
                        PairNumber = 1,
                        StartTime = new TimeSpan(8, 30, 0),
                        EndTime = new TimeSpan(10, 0, 0)
                    }
                });

            scheduleApiClientMock
                .Setup(x => x.GetDutyPersonsAsync())
                .ReturnsAsync(new List<DutyPersonViewModel>
                {
                    new DutyPersonViewModel { Id = 5, LastName = "Петров", FirstName = "Иван" },
                    new DutyPersonViewModel { Id = 3, LastName = "Андреева", FirstName = "Мария" }
                });

            var controller = new DutyScheduleController(scheduleApiClientMock.Object);

            var result = await controller.Create() as ViewResult;

            Assert.NotNull(result);

            var model = Assert.IsType<DutyScheduleCreateViewModel>(result!.Model);
            Assert.Equal(14 * 2, model.Cells.Count);
            Assert.Equal(2, model.LessonTimes.Count);
            Assert.Equal(2, model.DutyPersons.Count);
            Assert.Equal(2, model.DutyPersonItems.Count);

            Assert.Equal(1, model.LessonTimes[0].PairNumber);
            Assert.Equal(2, model.LessonTimes[1].PairNumber);
            Assert.Equal("3", model.DutyPersonItems[0].Value);
            Assert.Equal("5", model.DutyPersonItems[1].Value);
        }

        [Fact]
        public async Task Create_Post_ShouldReturnView_WhenSameDutyPersonChosenTwiceInCell()
        {
            var scheduleApiClientMock = new Mock<IScheduleApiClient>();
            SetupDutyDictionaries(scheduleApiClientMock);

            var controller = new DutyScheduleController(scheduleApiClientMock.Object);

            var model = new DutyScheduleCreateViewModel
            {
                Cells = new List<DutyScheduleCellEditViewModel>
                {
                    new DutyScheduleCellEditViewModel
                    {
                        Date = new DateTime(2026, 4, 28),
                        LessonTimeId = 1,
                        PairNumber = 1,
                        StartTime = new TimeSpan(8, 30, 0),
                        EndTime = new TimeSpan(10, 0, 0),
                        DutyPerson1Id = 7,
                        DutyPerson2Id = 7
                    }
                }
            };

            var result = await controller.Create(model) as ViewResult;

            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(
                controller.ModelState[string.Empty]!.Errors,
                e => e.ErrorMessage.Contains("Нельзя выбрать одного и того же дежурного дважды"));

            scheduleApiClientMock.Verify(
                x => x.CreateDutyScheduleAsync(It.IsAny<DutyScheduleBindingModel>()),
                Times.Never);
        }

        [Fact]
        public async Task Create_Post_ShouldCreateTwoSchedulesForOneCell_AndRedirect()
        {
            var scheduleApiClientMock = new Mock<IScheduleApiClient>();
            SetupDutyDictionaries(scheduleApiClientMock);

            scheduleApiClientMock
                .Setup(x => x.CreateDutyScheduleAsync(It.Is<DutyScheduleBindingModel>(m =>
                    m.Date == new DateTime(2026, 4, 29) &&
                    m.LessonTimeId == 1 &&
                    m.DutyPersonId == 3)))
                .ReturnsAsync(new DutyScheduleViewModel { Id = 1, DutyPersonId = 3 });

            scheduleApiClientMock
                .Setup(x => x.CreateDutyScheduleAsync(It.Is<DutyScheduleBindingModel>(m =>
                    m.Date == new DateTime(2026, 4, 29) &&
                    m.LessonTimeId == 1 &&
                    m.DutyPersonId == 5)))
                .ReturnsAsync(new DutyScheduleViewModel { Id = 2, DutyPersonId = 5 });

            var controller = new DutyScheduleController(scheduleApiClientMock.Object);

            var model = new DutyScheduleCreateViewModel
            {
                Cells = new List<DutyScheduleCellEditViewModel>
                {
                    new DutyScheduleCellEditViewModel
                    {
                        Date = new DateTime(2026, 4, 29),
                        LessonTimeId = 1,
                        PairNumber = 1,
                        StartTime = new TimeSpan(8, 30, 0),
                        EndTime = new TimeSpan(10, 0, 0),
                        DutyPerson1Id = 3,
                        DutyPerson2Id = 5
                    }
                }
            };

            var result = await controller.Create(model) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result!.ActionName);

            scheduleApiClientMock.Verify(
                x => x.CreateDutyScheduleAsync(It.IsAny<DutyScheduleBindingModel>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task Edit_Post_ShouldUpdateExistingCellAndCreateNewCell()
        {
            var scheduleApiClientMock = new Mock<IScheduleApiClient>();
            SetupDutyDictionaries(scheduleApiClientMock);

            scheduleApiClientMock
                .Setup(x => x.GetDutyScheduleAsync())
                .ReturnsAsync(new List<DutyScheduleViewModel>
                {
                    new DutyScheduleViewModel
                    {
                        Id = 10,
                        Date = new DateTime(2026, 4, 30),
                        LessonTimeId = 1,
                        DutyPersonId = 3,
                        DutyPersonName = "Петров Иван"
                    }
                });

            scheduleApiClientMock
                .Setup(x => x.UpdateDutyScheduleAsync(It.Is<DutyScheduleBindingModel>(m =>
                    m.Id == 10 &&
                    m.Date == new DateTime(2026, 4, 30) &&
                    m.LessonTimeId == 1 &&
                    m.DutyPersonId == 7)))
                .ReturnsAsync(new DutyScheduleViewModel { Id = 10, DutyPersonId = 7 });

            scheduleApiClientMock
                .Setup(x => x.CreateDutyScheduleAsync(It.Is<DutyScheduleBindingModel>(m =>
                    m.Date == new DateTime(2026, 5, 1) &&
                    m.LessonTimeId == 2 &&
                    m.DutyPersonId == 5)))
                .ReturnsAsync(new DutyScheduleViewModel { Id = 20, DutyPersonId = 5 });

            var controller = new DutyScheduleController(scheduleApiClientMock.Object);

            var model = new DutyScheduleCreateViewModel
            {
                Cells = new List<DutyScheduleCellEditViewModel>
                {
                    new DutyScheduleCellEditViewModel
                    {
                        Date = new DateTime(2026, 4, 30),
                        LessonTimeId = 1,
                        PairNumber = 1,
                        StartTime = new TimeSpan(8, 30, 0),
                        EndTime = new TimeSpan(10, 0, 0),
                        DutyPerson1Id = 7
                    },
                    new DutyScheduleCellEditViewModel
                    {
                        Date = new DateTime(2026, 5, 1),
                        LessonTimeId = 2,
                        PairNumber = 2,
                        StartTime = new TimeSpan(10, 15, 0),
                        EndTime = new TimeSpan(11, 45, 0),
                        DutyPerson1Id = 5
                    }
                }
            };

            var result = await controller.Edit(model) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result!.ActionName);

            scheduleApiClientMock.Verify(
                x => x.UpdateDutyScheduleAsync(It.IsAny<DutyScheduleBindingModel>()),
                Times.Once);

            scheduleApiClientMock.Verify(
                x => x.CreateDutyScheduleAsync(It.IsAny<DutyScheduleBindingModel>()),
                Times.Once);
        }

        private static void SetupDutyDictionaries(Mock<IScheduleApiClient> scheduleApiClientMock)
        {
            scheduleApiClientMock
                .Setup(x => x.GetLessonTimesAsync())
                .ReturnsAsync(new List<LessonTimeViewModel>
                {
                    new LessonTimeViewModel
                    {
                        Id = 1,
                        PairNumber = 1,
                        StartTime = new TimeSpan(8, 30, 0),
                        EndTime = new TimeSpan(10, 0, 0)
                    },
                    new LessonTimeViewModel
                    {
                        Id = 2,
                        PairNumber = 2,
                        StartTime = new TimeSpan(10, 15, 0),
                        EndTime = new TimeSpan(11, 45, 0)
                    }
                });

            scheduleApiClientMock
                .Setup(x => x.GetDutyPersonsAsync())
                .ReturnsAsync(new List<DutyPersonViewModel>
                {
                    new DutyPersonViewModel { Id = 3, LastName = "Петров", FirstName = "Иван" },
                    new DutyPersonViewModel { Id = 5, LastName = "Сидорова", FirstName = "Анна" },
                    new DutyPersonViewModel { Id = 7, LastName = "Иванова", FirstName = "Мария" }
                });
        }
    }
}
