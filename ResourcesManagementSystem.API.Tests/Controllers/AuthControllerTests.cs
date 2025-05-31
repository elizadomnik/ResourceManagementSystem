using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using ResourceManagementSystem.API.Controllers;
using ResourceManagementSystem.Core.Interfaces;
using ResourceManagementSystem.Core.DTOs.User;
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
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        private void SetupUserClaimsForController(Guid userId, string username, string email, string role = "User")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, username),
                new Claim(JwtRegisteredClaimNames.Email, email),
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
        public async Task Register_ValidModel_ReturnsOkObjectResult_WithAuthResponseDto()
        {
            // Arrange
            var registerDto = new UserRegisterDto { Username = "testuser", Email = "test@example.com", Password = "Password123!" };
            var expectedAuthResponse = new AuthResponseDto { UserId = Guid.NewGuid().ToString(), Username = "testuser", Email = "test@example.com", Token = "sample.jwt.token" };
            _mockAuthService.Setup(s => s.RegisterAsync(registerDto, "User"))
                .ReturnsAsync((true, expectedAuthResponse, Array.Empty<string>()));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualAuthResponse = Assert.IsType<AuthResponseDto>(okResult.Value);
            actualAuthResponse.Should().BeEquivalentTo(expectedAuthResponse, options => options.Excluding(o => o.ExpiresAt)); // Porównaj bez ExpiresAt, jeśli nie jest kluczowe dla tego testu
        }

        [Fact]
        public async Task Register_InvalidModel_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var registerDto = new UserRegisterDto { Username = "tu" };
            _controller.ModelState.AddModelError("Username", "Username is too short");

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsAssignableFrom<SerializableError>(badRequestResult.Value); 
        }

        [Fact]
        public async Task Register_ServiceFails_ReturnsBadRequestObjectResult_WithErrors()
        {
            // Arrange
            var registerDto = new UserRegisterDto { Username = "testuser", Email = "test@example.com", Password = "Password123!" };
            var errors = new[] { "Username already exists." };
            _mockAuthService.Setup(s => s.RegisterAsync(registerDto, "User"))
                .ReturnsAsync((false, null, errors));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value;
            Assert.NotNull(value);
            var errorsProperty = value.GetType().GetProperty("Errors");
            Assert.NotNull(errorsProperty);
            var actualErrors = errorsProperty.GetValue(value) as string[];
            actualErrors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkObjectResult_WithAuthResponseDto()
        {
            // Arrange
            var loginDto = new UserLoginDto { Email = "test@example.com", Password = "Password123!" };
            var expectedAuthResponse = new AuthResponseDto { UserId = Guid.NewGuid().ToString(), Username = "testuser", Email = "test@example.com", Token = "sample.jwt.token" };
            _mockAuthService.Setup(s => s.LoginAsync(loginDto))
                .ReturnsAsync((true, expectedAuthResponse, Array.Empty<string>()));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualAuthResponse = Assert.IsType<AuthResponseDto>(okResult.Value);
            actualAuthResponse.Should().BeEquivalentTo(expectedAuthResponse, options => options.Excluding(o => o.ExpiresAt));
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorizedObjectResult_WithErrors()
        {
            // Arrange
            var loginDto = new UserLoginDto { Email = "wrong@example.com", Password = "WrongPassword" };
            var errors = new[] { "Invalid email or password." };
            _mockAuthService.Setup(s => s.LoginAsync(loginDto))
                .ReturnsAsync((false, null, errors));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult.Value);
            var value = unauthorizedResult.Value;
            var errorsProperty = value.GetType().GetProperty("Errors");
            Assert.NotNull(errorsProperty);
            var actualErrors = errorsProperty.GetValue(value) as string[];
            actualErrors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public async Task Login_InvalidModel_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var loginDto = new UserLoginDto { Email = "notanemail" };
            _controller.ModelState.AddModelError("Email", "Invalid email format");

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsAssignableFrom<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task Logout_UserIsAuthenticated_CallsServiceAndReturnsOkWithMessage()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetupUserClaimsForController(userId, "testuser", "test@example.com");

            _mockAuthService.Setup(s => s.LogoutServiceSideAsync(userId.ToString()))
                .ReturnsAsync((true, Array.Empty<string>()));

            // Act
            var result = await _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            _mockAuthService.Verify(s => s.LogoutServiceSideAsync(userId.ToString()), Times.Once);
        }

        [Fact]
        public async Task Logout_UserIdClaimMissing_ReturnsUnauthorized()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim(ClaimTypes.Email, "test@example.com") }, "TestAuthType")) } // Brak NameIdentifier/Sub
            };

            // Act
            var result = await _controller.Logout();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User ID not found in token.", unauthorizedResult.Value);
            _mockAuthService.Verify(s => s.LogoutServiceSideAsync(It.IsAny<string>()), Times.Never); 
        }
    }
}