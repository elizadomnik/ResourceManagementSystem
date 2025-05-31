using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using ResourceManagementSystem.API.Controllers;
using ResourceManagementSystem.Core.Interfaces;
using ResourceManagementSystem.Core.DTOs.Resource;
using ResourceManagementSystem.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using System.IdentityModel.Tokens.Jwt; 

namespace ResourceManagementSystem.API.Tests.Controllers
{
    public class ResourcesControllerTests
    {
        private readonly Mock<IResourceService> _mockResourceService;
        private readonly ResourcesController _controller;
        private readonly Guid _defaultTestUserId = Guid.NewGuid();
        private readonly string _defaultTestUsername = "testuser";
        private readonly string _defaultTestUserRole = "User";
        private readonly string _adminTestUserRole = "Admin";


        public ResourcesControllerTests()
        {
            _mockResourceService = new Mock<IResourceService>();
            _controller = new ResourcesController(_mockResourceService.Object);
            SetupUserClaimsForController(_defaultTestUserId, _defaultTestUsername, _defaultTestUserRole);
        }

        private void SetupUserClaimsForController(Guid userId, string username, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, username),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task GetAllResources_UserIsAuthenticated_ReturnsOkObjectResult_WithListOfResources()
        {
            // Arrange
            var expectedResources = new List<ResourceDto>
            {
                new ResourceDto { Id = Guid.NewGuid(), Name = "Resource1" },
                new ResourceDto { Id = Guid.NewGuid(), Name = "Resource2" }
            };
            _mockResourceService.Setup(s => s.GetAllResourcesAsync()).ReturnsAsync(expectedResources);

            // Act
            var result = await _controller.GetAllResources();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResources = Assert.IsAssignableFrom<IEnumerable<ResourceDto>>(okResult.Value);
            actualResources.Should().BeEquivalentTo(expectedResources);
        }

        [Fact]
        public async Task GetResourceById_ExistingId_ReturnsOkObjectResult_WithResource()
        {
            // Arrange
            var resourceId = Guid.NewGuid();
            var expectedResource = new ResourceDto { Id = resourceId, Name = "Test Resource" };
            _mockResourceService.Setup(s => s.GetResourceByIdAsync(resourceId)).ReturnsAsync(expectedResource);

            // Act
            var result = await _controller.GetResourceById(resourceId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResource = Assert.IsType<ResourceDto>(okResult.Value);
            actualResource.Should().BeEquivalentTo(expectedResource);
        }

        [Fact]
        public async Task GetResourceById_NonExistingId_ReturnsNotFoundResult()
        {
            // Arrange
            var resourceId = Guid.NewGuid();
            _mockResourceService.Setup(s => s.GetResourceByIdAsync(resourceId)).ReturnsAsync((ResourceDto?)null);

            // Act
            var result = await _controller.GetResourceById(resourceId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateResource_ValidModel_ReturnsCreatedAtActionResult_WithResource()
        {
            // Arrange
            var createDto = new CreateResourceDto { Name = "New Resource", Type = ResourceType.Hardware };
            var createdResourceDto = new ResourceDto { Id = Guid.NewGuid(), Name = createDto.Name, Type = createDto.Type };
            _mockResourceService.Setup(s => s.CreateResourceAsync(createDto, _defaultTestUserId))
                .ReturnsAsync((true, createdResourceDto, Array.Empty<string>()));

            // Act
            var result = await _controller.CreateResource(createDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(ResourcesController.GetResourceById), createdAtActionResult.ActionName);
            var actualResource = Assert.IsType<ResourceDto>(createdAtActionResult.Value);
            actualResource.Should().BeEquivalentTo(createdResourceDto);
        }
        
        [Fact]
        public async Task CreateResource_InvalidModel_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var createDto = new CreateResourceDto { Name = "" };
            _controller.ModelState.AddModelError("Name", "The Name field is required.");

            // Act
            var result = await _controller.CreateResource(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsAssignableFrom<SerializableError>(badRequestResult.Value);
             _mockResourceService.Verify(s => s.CreateResourceAsync(It.IsAny<CreateResourceDto>(), It.IsAny<Guid>()), Times.Never);
        }


        [Fact]
        public async Task CreateResource_ServiceFails_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var createDto = new CreateResourceDto { Name = "New Resource" };
            var errors = new[] { "Creation failed" };
            _mockResourceService.Setup(s => s.CreateResourceAsync(createDto, _defaultTestUserId))
                .ReturnsAsync((false, null, errors));

            // Act
            var result = await _controller.CreateResource(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            var value = badRequestResult.Value;
            var errorsProperty = value.GetType().GetProperty("Errors");
            Assert.NotNull(errorsProperty);
            var actualErrors = errorsProperty.GetValue(value) as string[];
            actualErrors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public async Task UpdateResource_ExistingIdAndValidModel_ReturnsOkObjectResult()
        {
            // Arrange
            var resourceId = Guid.NewGuid();
            var updateDto = new UpdateResourceDto { Name = "Updated Resource" };
            var updatedResourceDto = new ResourceDto { Id = resourceId, Name = updateDto.Name };
             _mockResourceService.Setup(s => s.UpdateResourceAsync(resourceId, updateDto, _defaultTestUserId))
                .ReturnsAsync((true, updatedResourceDto, Array.Empty<string>()));

            // Act
            var result = await _controller.UpdateResource(resourceId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResource = Assert.IsType<ResourceDto>(okResult.Value);
            actualResource.Should().BeEquivalentTo(updatedResourceDto);
        }
        
        [Fact]
        public async Task UpdateResource_InvalidModel_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var resourceId = Guid.NewGuid();
            var updateDto = new UpdateResourceDto { Name = "" }; // Pusta nazwa
            _controller.ModelState.AddModelError("Name", "The Name field is required.");


            // Act
            var result = await _controller.UpdateResource(resourceId, updateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsAssignableFrom<SerializableError>(badRequestResult.Value);
            _mockResourceService.Verify(s => s.UpdateResourceAsync(It.IsAny<Guid>(), It.IsAny<UpdateResourceDto>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateResource_NonExistingId_ReturnsNotFoundResult()
        {
            // Arrange
            var resourceId = Guid.NewGuid();
            var updateDto = new UpdateResourceDto { Name = "Updated Resource" };
            _mockResourceService.Setup(s => s.UpdateResourceAsync(resourceId, updateDto, _defaultTestUserId))
                .ReturnsAsync((false, null, new[] { "Resource not found." }));

            // Act
            var result = await _controller.UpdateResource(resourceId, updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        
        [Fact]
        public async Task UpdateResource_ServiceFails_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var resourceId = Guid.NewGuid();
            var updateDto = new UpdateResourceDto { Name = "Updated Resource" };
            var errors = new[] { "Update failed due to conflict" }; // Inny błąd niż "Resource not found"
            _mockResourceService.Setup(s => s.UpdateResourceAsync(resourceId, updateDto, _defaultTestUserId))
                .ReturnsAsync((false, null, errors));

            // Act
            var result = await _controller.UpdateResource(resourceId, updateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            var value = badRequestResult.Value;
            var errorsProperty = value.GetType().GetProperty("Errors");
            Assert.NotNull(errorsProperty);
            var actualErrors = errorsProperty.GetValue(value) as string[];
            actualErrors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public async Task DeleteResource_UserIsAdminAndExistingId_ReturnsNoContentResult()
        {
            // Arrange
            SetupUserClaimsForController(_defaultTestUserId, "adminUser", _adminTestUserRole); 
            var resourceId = Guid.NewGuid();
            _mockResourceService.Setup(s => s.DeleteResourceAsync(resourceId, _defaultTestUserId))
                .ReturnsAsync((true, Array.Empty<string>()));

            // Act
            var result = await _controller.DeleteResource(resourceId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockResourceService.Verify(s => s.DeleteResourceAsync(resourceId, _defaultTestUserId), Times.Once);
        }
        
        [Fact]
        public async Task DeleteResource_UserIsAdminAndNonExistingId_ReturnsNotFoundResult()
        {
            // Arrange
            SetupUserClaimsForController(_defaultTestUserId, "adminUser", _adminTestUserRole);
            var resourceId = Guid.NewGuid();
            _mockResourceService.Setup(s => s.DeleteResourceAsync(resourceId, _defaultTestUserId))
                .ReturnsAsync((false, new[] { "Resource not found." }));

            // Act
            var result = await _controller.DeleteResource(resourceId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteResource_UserIsAdminAndServiceFails_ReturnsBadRequestObjectResult()
        {
            // Arrange
            SetupUserClaimsForController(_defaultTestUserId, "adminUser", _adminTestUserRole);
            var resourceId = Guid.NewGuid();
            var errors = new[] { "Deletion failed" };
            _mockResourceService.Setup(s => s.DeleteResourceAsync(resourceId, _defaultTestUserId))
                .ReturnsAsync((false, errors));
            
            // Act
            var result = await _controller.DeleteResource(resourceId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            var value = badRequestResult.Value;
            var errorsProperty = value.GetType().GetProperty("Errors");
            Assert.NotNull(errorsProperty);
            var actualErrors = errorsProperty.GetValue(value) as string[];
            actualErrors.Should().BeEquivalentTo(errors);
        }
        
    }
}