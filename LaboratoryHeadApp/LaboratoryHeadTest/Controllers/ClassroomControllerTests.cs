using LaboratoryHeadApp.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using MolServiceContracts.BindingModels;
using MolServiceContracts.ViewModels;
using MOLServiceWebClient;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaboratoryHeadTest.Controllers
{
    public class ClassroomControllerTests
    {
        [Fact]
        public async Task Edit_Get_ShouldReturnNotFound_WhenClassroomDoesNotExist()
        {
            var molApiClientMock = new Mock<IMolApiClient>();

            molApiClientMock
                .Setup(x => x.GetClassroomAsync(99))
                .ReturnsAsync((ClassroomViewModel?)null);

            var controller = CreateController(molApiClientMock);

            var result = await controller.Edit(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_Post_ShouldReturnView_WhenModelStateIsInvalid_AndNotCallApi()
        {
            var molApiClientMock = new Mock<IMolApiClient>();
            var controller = CreateController(molApiClientMock);

            controller.ModelState.AddModelError("Number", "Номер обязателен");

            var model = new ClassroomBindingModel
            {
                Number = "",
                Capacity = 20
            };

            var result = await controller.Create(model) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal(model, result!.Model);

            molApiClientMock.Verify(x => x.CreateClassroomAsync(It.IsAny<ClassroomBindingModel>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_ShouldReturnViewWithModelError_WhenApiReturnsFalse()
        {
            var molApiClientMock = new Mock<IMolApiClient>();

            molApiClientMock
                .Setup(x => x.UpdateClassroomAsync(It.IsAny<ClassroomBindingModel>()))
                .ReturnsAsync(false);

            var controller = CreateController(molApiClientMock);

            var model = new ClassroomBindingModel
            {
                Id = 5,
                Number = "405",
                Capacity = 30
            };

            var result = await controller.Edit(model) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal(model, result!.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(
                controller.ModelState[string.Empty]!.Errors,
                e => e.ErrorMessage == "Не удалось обновить аудиторию");
        }

        [Fact]
        public async Task ImportFromCore_ShouldSetSuccessMessage_AndRedirect_WhenImportSucceeded()
        {
            var molApiClientMock = new Mock<IMolApiClient>();

            molApiClientMock
                .Setup(x => x.ImportClassroomsFromCoreAsync())
                .ReturnsAsync(true);

            var controller = CreateController(molApiClientMock);

            var result = await controller.ImportFromCore() as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result!.ActionName);
            Assert.Equal("Синхронизация аудиторий из core успешно выполнена.", controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task Delete_ShouldPutExceptionMessageIntoTempData_AndRedirect()
        {
            var molApiClientMock = new Mock<IMolApiClient>();

            molApiClientMock
                .Setup(x => x.DeleteClassroomAsync(7))
                .ThrowsAsync(new Exception("Аудитория используется в связанных данных"));

            var controller = CreateController(molApiClientMock);

            var result = await controller.Delete(7) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result!.ActionName);
            Assert.Equal("Аудитория используется в связанных данных", controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Create_Post_ShouldRedirectToIndex_WhenApiReturnsTrue()
        {
            var molApiClientMock = new Mock<IMolApiClient>();

            molApiClientMock
                .Setup(x => x.CreateClassroomAsync(It.Is<ClassroomBindingModel>(m =>
                    m.Number == "501" &&
                    m.Capacity == 18)))
                .ReturnsAsync(true);

            var controller = CreateController(molApiClientMock);

            var model = new ClassroomBindingModel
            {
                Number = "501",
                Capacity = 18
            };

            var result = await controller.Create(model) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result!.ActionName);

            molApiClientMock.Verify(x => x.CreateClassroomAsync(It.IsAny<ClassroomBindingModel>()), Times.Once);
        }

        private static ClassroomController CreateController(Mock<IMolApiClient> molApiClientMock)
        {
            var controller = new ClassroomController(molApiClientMock.Object);

            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            return controller;
        }
    }
}
