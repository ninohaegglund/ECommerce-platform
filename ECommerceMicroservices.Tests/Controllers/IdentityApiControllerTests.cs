using FluentAssertions;
using IdentityService.API.Controllers;
using IdentityService.API.DTOs;
using IdentityService.API.Interfaces;
using IdentityService.API.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ECommerceMicroservices.Tests.Controllers;

public class IdentityApiControllerTests
{
    [Fact]
    public async Task Register_ReturnsOk_WhenRegistrationSucceeds()
    {
        var request = new RegisterRequestDto
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@test.local",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };
        var response = new RegisterResponseDto
        {
            Message = "Account created.",
            EmailVerificationRequired = true,
            User = new UserDto
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            }
        };

        var authService = new Mock<IAuthService>();
        authService
            .Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(response);

        var controller = new AuthController(authService.Object);

        var result = await controller.Register(request);

        var ok = result.Should().BeOfType<OkObjectResult>().Which;
        ok.Value.Should().BeSameAs(response);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenServiceRejectsRequest()
    {
        var request = new RegisterRequestDto();
        var authService = new Mock<IAuthService>();
        authService
            .Setup(x => x.RegisterAsync(request))
            .ThrowsAsync(new InvalidOperationException("Email already exists."));

        var controller = new AuthController(authService.Object);

        var result = await controller.Register(request);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Which;
        badRequest.Value.Should().BeEquivalentTo(new { message = "Email already exists." });
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
    {
        var request = new LoginRequestDto
        {
            Email = "ada@test.local",
            Password = "wrong-password"
        };

        var authService = new Mock<IAuthService>();
        authService
            .Setup(x => x.LoginAsync(request))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials."));

        var controller = new AuthController(authService.Object);

        var result = await controller.Login(request);

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Which;
        unauthorized.Value.Should().BeEquivalentTo(new { message = "Invalid credentials." });
    }

    [Fact]
    public async Task Users_GetById_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var controller = new UsersController(userRepository.Object);

        var result = await controller.GetById(userId);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Users_GetById_ReturnsUserWithRoles()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Grace",
            LastName = "Hopper",
            Email = "grace@test.local",
            UserRoles =
            [
                new UserRole { Role = new Role { Name = "Admin" } },
                new UserRole { Role = new Role { Name = "Customer" } }
            ]
        };

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        var controller = new UsersController(userRepository.Object);

        var result = await controller.GetById(user.Id);

        var ok = result.Should().BeOfType<OkObjectResult>().Which;
        var response = ok.Value.Should().BeOfType<UserDto>().Which;
        response.Id.Should().Be(user.Id);
        response.Email.Should().Be(user.Email);
        response.Roles.Should().Equal("Admin", "Customer");
    }
}
