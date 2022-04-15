using System;
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

public class SessionControllerTests
{
    #region snippet_SessionControllerTests
    [Theory]
    [AutoDomainData]
    public async Task IndexReturnsARedirectToIndexHomeWhenIdIsNull([Greedy]SessionController controller)
    {
        // Arrange

        // Act
        var result = await controller.Index(id: null);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        redirectToActionResult.ControllerName.Should().Be("Home");
        redirectToActionResult.ActionName.Should().Be("Index");
    }

    [Theory]
    [AutoDomainData]
    public async Task IndexReturnsContentWithSessionNotFoundWhenSessionNotFound(
        [Frozen] Mock<IBrainstormSessionRepository> mockRepo,
        [Greedy] SessionController controller)
    {
        // Arrange
        int testSessionId = 1;
        mockRepo.Setup(repo => repo.GetByIdAsync(testSessionId))
            .ReturnsAsync((BrainstormSession)null);

        // Act
        var result = await controller.Index(testSessionId);

        // Assert
        var contentResult = Assert.IsType<ContentResult>(result);
        contentResult.Content.Should().Be("Session not found.");
    }

    [Theory]
    [AutoDomainData]
    public async Task IndexReturnsViewResultWithStormSessionViewModel(
        List<BrainstormSession> testSessions,
        [Frozen] Mock<IBrainstormSessionRepository> mockRepo,
        [Greedy] SessionController controller)
    {
        // Arrange
        const int testSessionId = 1;
        const string testName = "Test One";
        DateTimeOffset dataCreated = DateTimeOffset.Parse("Oct 28 1981");
        testSessions[0].Id = testSessionId;
        testSessions[0].Name = testName;
        testSessions[0].DateCreated = dataCreated;
        mockRepo.Setup(repo => repo.GetByIdAsync(testSessionId))
            .ReturnsAsync(testSessions.Find(
                s => s.Id == testSessionId));

        // Act
        var result = await controller.Index(testSessionId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<StormSessionViewModel>(
            viewResult.ViewData.Model);
        model.Name.Should().Be(testName);
        model.DateCreated.Day.Should().Be(dataCreated.Day);
        model.Id.Should().Be(testSessionId);
    }
    #endregion
}
