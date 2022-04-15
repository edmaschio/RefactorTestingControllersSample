﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TestingControllersSample.Api;
using TestingControllersSample.ClientModels;
using TestingControllersSample.Core.Interfaces;
using TestingControllersSample.Core.Model;
using Xunit;

namespace TestingControllersSample.Tests.UnitTests
{
    public class ApiIdeasControllerTests
    {
        #region snippet_ApiIdeasControllerTests1
        [Fact]
        public async Task Create_ReturnsBadRequest_GivenInvalidModel()
        {
            // Arrange & Act
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            var controller = new IdeasController(mockRepo.Object);
            controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await controller.Create(model: null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
        #endregion

        #region snippet_ApiIdeasControllerTests2
        [Fact]
        public async Task Create_ReturnsHttpNotFound_ForInvalidSession()
        {
            // Arrange
            int testSessionId = 123;
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            mockRepo.Setup(repo => repo.GetByIdAsync(testSessionId))
                .ReturnsAsync((BrainstormSession)null);
            var controller = new IdeasController(mockRepo.Object);

            // Act
            var result = await controller.Create(new NewIdeaModel());

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
        #endregion

        #region snippet_ApiIdeasControllerTests3
        [Theory]
        [AutoDomainData]
        public async Task Create_ReturnsNewlyCreatedIdeaForSession(NewIdeaModel newIdea, BrainstormSession testSession)
        {
            // Arrange
            newIdea.SessionId = testSession.Id;
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            mockRepo.Setup(repo => repo.GetByIdAsync(newIdea.SessionId))
                .ReturnsAsync(testSession);
            var controller = new IdeasController(mockRepo.Object);

            mockRepo.Setup(repo => repo.UpdateAsync(testSession))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await controller.Create(newIdea);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnSession = Assert.IsType<BrainstormSession>(okResult.Value);
            mockRepo.Verify();
            returnSession.Ideas.Count.Should().Be(2);
            returnSession.Ideas.LastOrDefault().Name.Should().Be(newIdea.Name);
            returnSession.Ideas.LastOrDefault().Description.Should().Be(newIdea.Description);
        }
        #endregion

        #region snippet_ApiIdeasControllerTests4
        [Fact]
        public async Task ForSession_ReturnsHttpNotFound_ForInvalidSession()
        {
            // Arrange
            int testSessionId = 123;
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            mockRepo.Setup(repo => repo.GetByIdAsync(testSessionId))
                .ReturnsAsync((BrainstormSession)null);
            var controller = new IdeasController(mockRepo.Object);

            // Act
            var result = await controller.ForSession(testSessionId);

            // Assert
            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(testSessionId, notFoundObjectResult.Value);
        }
        #endregion

        #region snippet_ApiIdeasControllerTests5
        [Theory]
        [AutoDomainData]
        public async Task ForSession_ReturnsIdeasForSession(BrainstormSession testSession)
        {
            // Arrange
            string ideaName = "One";
            testSession.Ideas[0].Name = ideaName;
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            mockRepo.Setup(repo => repo.GetByIdAsync(testSession.Id))
                .ReturnsAsync(testSession);
            var controller = new IdeasController(mockRepo.Object);

            // Act
            var result = await controller.ForSession(testSession.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<IdeaDTO>>(okResult.Value);
            var idea = returnValue.FirstOrDefault();
            idea.Name.Should().Be(ideaName);
        }
        #endregion

        #region snippet_ForSessionActionResult_ReturnsNotFoundObjectResultForNonexistentSession
        [Fact]
        public async Task ForSessionActionResult_ReturnsNotFoundObjectResultForNonexistentSession()
        {
            // Arrange
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            var controller = new IdeasController(mockRepo.Object);
            var nonExistentSessionId = 999;

            // Act
            var result = await controller.ForSessionActionResult(nonExistentSessionId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<List<IdeaDTO>>>(result);
            Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        }
        #endregion

        #region snippet_ForSessionActionResult_ReturnsIdeasForSession
        [Theory]
        [AutoDomainData]
        public async Task ForSessionActionResult_ReturnsIdeasForSession(BrainstormSession testSession)
        {
            // Arrange
            string ideasName = "One";
            int testSessionId = 123;
            testSession.Ideas[0].Name = ideasName;
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            mockRepo.Setup(repo => repo.GetByIdAsync(testSessionId))
                .ReturnsAsync(testSession);
            var controller = new IdeasController(mockRepo.Object);

            // Act
            var result = await controller.ForSessionActionResult(testSessionId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<List<IdeaDTO>>>(result);
            var returnValue = Assert.IsType<List<IdeaDTO>>(actionResult.Value);
            var idea = returnValue.FirstOrDefault();
            idea.Name.Should().Be(ideasName);
        }
        #endregion

        #region snippet_CreateActionResult_ReturnsBadRequest_GivenInvalidModel
        [Fact]
        public async Task CreateActionResult_ReturnsBadRequest_GivenInvalidModel()
        {
            // Arrange & Act
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            var controller = new IdeasController(mockRepo.Object);
            controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await controller.CreateActionResult(model: null);

            // Assert
            var actionResult = Assert.IsType<ActionResult<BrainstormSession>>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }
        #endregion

        #region snippet_CreateActionResult_ReturnsNotFoundObjectResultForNonexistentSession
        [Theory]
        [AutoData]
        public async Task CreateActionResult_ReturnsNotFoundObjectResultForNonexistentSession(NewIdeaModel newIdea)
        {
            // Arrange
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            var controller = new IdeasController(mockRepo.Object);

            // Act
            var result = await controller.CreateActionResult(newIdea);

            // Assert
            var actionResult = Assert.IsType<ActionResult<BrainstormSession>>(result);
            Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        }
        #endregion

        #region snippet_CreateActionResult_ReturnsNewlyCreatedIdeaForSession
        [Theory]
        [AutoDomainData]
        public async Task CreateActionResult_ReturnsNewlyCreatedIdeaForSession(
            NewIdeaModel newIdea,
            BrainstormSession testSession)
        {
            // Arrange
            newIdea.SessionId = testSession.Id;
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            mockRepo.Setup(repo => repo.GetByIdAsync(newIdea.SessionId))
                .ReturnsAsync(testSession);
            var controller = new IdeasController(mockRepo.Object);
            mockRepo.Setup(repo => repo.UpdateAsync(testSession))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await controller.CreateActionResult(newIdea);

            // Assert
            var actionResult = Assert.IsType<ActionResult<BrainstormSession>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<BrainstormSession>(createdAtActionResult.Value);
            mockRepo.Verify();
            returnValue.Ideas.Count.Should().Be(2);
            returnValue.Ideas.LastOrDefault().Name.Should().Be(newIdea.Name);
            returnValue.Ideas.LastOrDefault().Description.Should().Be(newIdea.Description);
        }
        #endregion
    }
}
