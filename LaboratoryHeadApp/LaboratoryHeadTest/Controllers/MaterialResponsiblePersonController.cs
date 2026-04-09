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
    public class MaterialResponsiblePersonControllerTests
    {
        [Fact]
        public async Task Edit_Get_ShouldReturnNotFound_WhenMolDoesNotExist()
        {
            var molApiClientMock = new Mock<IMolApiClient>();

            molApiClientMock
                .Setup(x => x.GetMaterialResponsiblePersonAsync(99))
                .ReturnsAsync((MaterialResponsiblePersonViewModel?)null);

            var controller = CreateController(molApiClientMock);

            var result = await controller.Edit(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_Post_ShouldReturnViewAndPutExceptionIntoModelState_WhenApiThrows()
        {
            var molApiClientMock = new Mock<IMolApiClient>();

            molApiClientMock
                .Setup(x => x.CreateMaterialResponsiblePersonAsync(It.IsAny<MaterialResponsiblePersonBindingModel>()))
                .ThrowsAsync(new Exception("МОЛ с таким email уже существует"));

            var controller = CreateController(molApiClientMock);

            var model = new MaterialResponsiblePersonBindingModel
            {
                FullName = "Иванова И.И.",
                Position = "Лаборант",
                Phone = "+79990001122",
                Email = "ivanova@test.ru"
            };

            var result = await controller.Create(model) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal(model, result!.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(
                controller.ModelState[string.Empty]!.Errors,
                e => e.ErrorMessage == "МОЛ с таким email уже существует");
        }

        [Fact]
        public async Task Edit_Post_ShouldRedirectToIndex_WhenApiReturnsTrue()
        {
            var molApiClientMock = new Mock<IMolApiClient>();

            molApiClientMock
                .Setup(x => x.UpdateMaterialResponsiblePersonAsync(It.Is<MaterialResponsiblePersonBindingModel>(m =>
                    m.Id == 5 &&
                    m.FullName == "Петров П.П." &&
                    m.Position == "Старший лаборант")))
                .ReturnsAsync(true);

            var controller = CreateController(molApiClientMock);

            var model = new MaterialResponsiblePersonBindingModel
            {
                Id = 5,
                FullName = "Петров П.П.",
                Position = "Старший лаборант",
                Phone = "+79995554433",
                Email = "petrov@test.ru"
            };

            var result = await controller.Edit(model) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result!.ActionName);

            molApiClientMock.Verify(
                x => x.UpdateMaterialResponsiblePersonAsync(It.IsAny<MaterialResponsiblePersonBindingModel>()),
                Times.Once);
        }

        [Fact]
        public async Task Delete_Post_ShouldWriteErrorToTempData_WhenApiThrows()
        {
            var molApiClientMock = new Mock<IMolApiClient>();

            molApiClientMock
                .Setup(x => x.DeleteMaterialResponsiblePersonAsync(7))
                .ThrowsAsync(new Exception("Нельзя удалить МОЛ, пока за ним закреплено оборудование"));

            var controller = CreateController(molApiClientMock);

            var result = await controller.Delete(7) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result!.ActionName);
            Assert.Equal(
                "Нельзя удалить МОЛ, пока за ним закреплено оборудование",
                controller.TempData["ErrorMessage"]);
        }

        private static MaterialResponsiblePersonController CreateController(Mock<IMolApiClient> molApiClientMock)
        {
            var controller = new MaterialResponsiblePersonController(molApiClientMock.Object);

            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            return controller;
        }
    }
}
