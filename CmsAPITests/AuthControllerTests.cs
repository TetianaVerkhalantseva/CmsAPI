using CmsAPI.Controllers;
using CmsAPI.Models;
using CmsAPI.Models.Entities;
using CmsAPI.Services.AuthServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CmsAPITests
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _userManagerMock = MockUserManager();
            _controller = new AuthController(_authServiceMock.Object, _userManagerMock.Object);
        }

        private static Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        // POST: /api/Auth/Register
        [Fact]
        public async Task RegisterUser_ReturnsConflict_WhenUserAlreadyExists()
        {
            // Arrange
            var userDto = new LoginDto { Username = "existingUser", Password = "password" };
            _userManagerMock.Setup(manager => manager.FindByNameAsync(userDto.Username))
                .ReturnsAsync(new User { UserName = userDto.Username });

            // Act
            var result = await _controller.RegisterUser(userDto);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("User already exists. Please log in instead.", ((ConflictObjectResult)result).Value);
        }
        
        [Fact]
        public async Task RegisterUser_ReturnsOk_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var userDto = new LoginDto { Username = "newUser", Password = "password" };
            _userManagerMock.Setup(manager => manager.FindByNameAsync(userDto.Username))
                .ReturnsAsync((User)null);
            _authServiceMock.Setup(service => service.RegisterUser(userDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.RegisterUser(userDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = result.Value as AuthResponseDto;
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            Assert.Equal("User successfully registered!", response.Message);
            Assert.Null(response.Token);
        }

        [Fact]
        public async Task RegisterUser_ReturnsBadRequest_WhenRegistrationFails()
        {
            // Arrange
            var userDto = new LoginDto { Username = "newUser", Password = "password" };
            _userManagerMock.Setup(manager => manager.FindByNameAsync(userDto.Username))
                .ReturnsAsync((User)null);
            _authServiceMock.Setup(service => service.RegisterUser(userDto)).ReturnsAsync(false);

            // Act
            var result = await _controller.RegisterUser(userDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Something went wrong...", ((BadRequestObjectResult)result).Value);
        }

        // POST: /api/Auth/Login
        [Fact]
        public async Task Login_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var userDto = new LoginDto { Username = "user", Password = "password" };
            _controller.ModelState.AddModelError("TestError", "Invalid ModelState");

            // Act
            var result = await _controller.Login(userDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid ModelState...", ((BadRequestObjectResult)result).Value);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenUserDoesNotExist()
        {
            // Arrange
            var userDto = new LoginDto { Username = "nonExistentUser", Password = "password" };
            _userManagerMock.Setup(manager => manager.FindByNameAsync(userDto.Username))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login(userDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User does not exist...", ((BadRequestObjectResult)result).Value);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenLoginFails()
        {
            // Arrange
            var userDto = new LoginDto { Username = "user", Password = "password" };
            var user = new User { UserName = userDto.Username };
            _userManagerMock.Setup(manager => manager.FindByNameAsync(userDto.Username))
                .ReturnsAsync(user);
            _authServiceMock.Setup(service => service.Login(userDto)).ReturnsAsync(false);

            // Act
            var result = await _controller.Login(userDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Something went wrong when logging in...", ((BadRequestObjectResult)result).Value);
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenLoginIsSuccessful()
        {
            // Arrange
            var userDto = new LoginDto { Username = "user", Password = "password" };
            var user = new User { UserName = userDto.Username };
            _userManagerMock.Setup(manager => manager.FindByNameAsync(userDto.Username))
                .ReturnsAsync(user);
            _authServiceMock.Setup(service => service.Login(userDto)).ReturnsAsync(true);
            _authServiceMock.Setup(service => service.GenerateTokenString(user)).Returns("MockedToken");

            // Act
            var result = await _controller.Login(userDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = result.Value as AuthResponseDto;
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            Assert.Equal("MockedToken", response.Token);
            Assert.Equal("Login Successful!", response.Message);
        }
    }
}
