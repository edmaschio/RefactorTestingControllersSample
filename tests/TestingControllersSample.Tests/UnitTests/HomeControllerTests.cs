using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TestingControllersSample.Controllers;
using TestingControllersSample.Core.Interfaces;
using TestingControllersSample.Core.Model;
using TestingControllersSample.ViewModels;
using Xunit;

namespace TestingControllersSample.Tests.UnitTests;

public class HomeControllerTests
{
    #region snippet_Index_ReturnsAViewResult_WithAListOfBrainstormSessions
    [Theory]
    [AutoDomainData]
    public async Task Index_ReturnsAViewResult_WithAListOfBrainstormSessions(
        [CollectionSize(2)] List<BrainstormSession> testSessions,
        [Frozen] Mock<IBrainstormSessionRepository> mockRepo,
        [Greedy] HomeController controller)
    {
        // Arrange
        mockRepo.Setup(repo => repo.ListAsync())
            .ReturnsAsync(testSessions);

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<StormSessionViewModel>>(
            viewResult.ViewData.Model);
        model.Count().Should().Be(2);
    }
    #endregion

    #region snippet_ModelState_ValidOrInvalid
    [Theory]
    [AutoDomainData]
    public async Task IndexPost_ReturnsBadRequestResult_WhenModelStateIsInvalid(
        [CollectionSize(6)]List<BrainstormSession> testSessions,
        [Frozen] Mock<IBrainstormSessionRepository> mockRepo,
        [Greedy] HomeController controller,
        HomeController.NewSessionModel newSession)
    {
        // Arrange
        mockRepo.Setup(repo => repo.ListAsync())
            .ReturnsAsync(testSessions);
        controller.ModelState.AddModelError("SessionName", "Required");

        // Act
        var result = await controller.Index(newSession);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<SerializableError>(badRequestResult.Value);
    }

    [Theory]
    [AutoDomainData]
    public async Task IndexPost_ReturnsARedirectAndAddsSession_WhenModelStateIsValid(
        [Frozen] Mock<IBrainstormSessionRepository> mockRepo,
        [Greedy] HomeController controller,
        HomeController.NewSessionModel newSession)
    {
        // Arrange
        mockRepo.Setup(repo => repo.AddAsync(It.IsAny<BrainstormSession>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        var result = await controller.Index(newSession);

        // Assert
        RedirectToActionResult redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

        redirectToActionResult.ControllerName.Should().BeNull();
        redirectToActionResult.ActionName.Should().Be("Index");
        mockRepo.Verify();
    }
    #endregion
}
