using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Moq;
using OrbitPM.Controllers;
using OrbitPM.Data;
using OrbitPM.Models;
using System.Security.Claims;
using Xunit;

namespace OrbitPM.Tests
{
    public class AccountControllerTests
    {
        [Fact]
        public async Task Register_Post_ValidModel_ReturnsRedirectToIndex()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "RegisterTestDatabase")
                .Options;

            // Mocking Authentication Services
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(s => s.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            // Mocking IUrlHelper for RedirectToAction
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns("callbackUrl");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(s => s.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);
            
            // Adding UrlHelperFactory Mock to avoid dependency issues
            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            serviceProviderMock
                .Setup(s => s.GetService(typeof(IUrlHelperFactory)))
                .Returns(urlHelperFactoryMock.Object);

            var httpContext = new DefaultHttpContext { RequestServices = serviceProviderMock.Object };

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new AccountController(context)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = httpContext
                    },
                    Url = urlHelperMock.Object
                };

                var viewModel = new RegisterViewModel
                {
                    FullName = "Test User",
                    Email = "test@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!",
                    Role = "Supervisor" // Attempting to register as Supervisor
                };

                // Act
                var result = await controller.Register(viewModel);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);
                Assert.Equal("StudentDashboard", redirectResult.ControllerName);

                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
                Assert.NotNull(user);
                Assert.Equal("Test User", user.FullName);
                Assert.Equal("Student", user.Role); // Should be forced to Student
                
                authServiceMock.Verify(s => s.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Once);
            }
        }

        [Fact]
        public async Task Register_Post_ExistingEmail_ReturnsViewWithModelError()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ExistingEmailDatabase_" + Guid.NewGuid().ToString())
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Users.Add(new ApplicationUser { Email = "existing@example.com", PasswordHash = "hash" });
                await context.SaveChangesAsync();

                var controller = new AccountController(context);
                var viewModel = new RegisterViewModel
                {
                    FullName = "New User",
                    Email = "existing@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!",
                    Role = "Student"
                };

                // Act
                var result = await controller.Register(viewModel);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.False(controller.ModelState.IsValid);
                Assert.True(controller.ModelState.ContainsKey("Email"));
            }
        }
    }
}
