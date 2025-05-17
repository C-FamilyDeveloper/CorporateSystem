using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoFixture.Xunit2;
using CorporateSystem.Auth.Domain.Entities;
using CorporateSystem.Auth.Domain.Enums;
using CorporateSystem.Auth.Infrastructure;
using CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;
using CorporateSystem.Auth.Services.Exceptions;
using CorporateSystem.Auth.Services.Options;
using CorporateSystem.Auth.Services.Services.GrpcServices;
using CorporateSystem.Auth.Services.Services.Implementations;
using CorporateSystem.Auth.Services.Services.Interfaces;
using CorporateSystem.Auth.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace CorporateSystem.Auth.Tests.UnitTests.Services;

public class UserServiceTests : IClassFixture<TestFixture>
{
    private readonly string _testSecretKey = "a-very-long-and-secure-test-secret-key";
    
    [Theory, AutoData]
    public async Task SuccessRegisterAsync_ValidInput_ValidSuccessCode_ShouldCreateUser(int successCode)
    {
        // Act
        using var testFixture = new TestFixture();
        var registrationCodesRepositoryMock = new Mock<IRegistrationCodesRepository>();
    
        var email = "test@bobr.ru";
        var password = "password";
        
        var dataContext = testFixture.GetService<DataContext>();
        registrationCodesRepositoryMock
            .Setup(x => x.GetAsync(new object[] { email }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => successCode);
        
        var userService = new UserService(
            testFixture.GetService<IContextFactory>(),
            testFixture.GetService<IPasswordHasher>(),
            registrationCodesRepositoryMock.Object, 
            null,
            testFixture.GetService<IOptions<JwtToken>>(),
            new OptionsWrapper<NotificationOptions>(null),
            null);
        
        // Arrange
        var request = new SuccessRegisterUserDto(email, password, successCode, string.Empty, string.Empty, Gender.Female);
        
        await userService.SuccessRegisterAsync(request);
        
        // Assert
        registrationCodesRepositoryMock.Verify(x => 
            x.DeleteAsync(new object[] { email }, It.IsAny<CancellationToken>()), Times.Once);
        
        Assert.Single(dataContext.Users);
        Assert.Equal(email, dataContext.Users.Single().Email);
    }
    
    [Theory, AutoData]
    public async Task SuccessRegisterAsync_ValidInput_InvalidSuccessCode_ShouldThrowExceptionWithStatusCode(int successCode)
    {
        // Act
        using var testFixture = new TestFixture();
        var registrationCodesRepositoryMock = new Mock<IRegistrationCodesRepository>();
    
        registrationCodesRepositoryMock
            .Setup(x =>
                x.GetAsync(It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);
        // Arrange
        var userService = new UserService(
            testFixture.GetService<IContextFactory>(),
            testFixture.GetService<IPasswordHasher>(),
            registrationCodesRepositoryMock.Object, 
            null,
            testFixture.GetService<IOptions<JwtToken>>(),
            new OptionsWrapper<NotificationOptions>(null),
            new LoggerFactory().CreateLogger<UserService>());
        
        var email = "test@bobr.ru";
        var password = "password";
        
        var request = new SuccessRegisterUserDto(email, password, successCode, string.Empty, string.Empty, Gender.Female);
        // Assert
    
        await Assert.ThrowsAsync<InvalidRegistrationException>(async () =>
        {
            await userService.SuccessRegisterAsync(request);
        });
    }
    
    [Fact]
    public async Task AuthenticateAsync_ValidEmailAndPassword_UserExists_ShouldReturnJwtToken()
    {
        // Act
        using var testFixture = new TestFixture();
        var email = "test@bobr.ru";
        var password = "password";
    
        var dataContext = testFixture.GetService<DataContext>();
        dataContext.Users.Add(new User
        {
            Email = email,
            Password = testFixture.GetService<IPasswordHasher>().HashPassword(password),
            FirstName = string.Empty,
            LastName = string.Empty
        });

        await dataContext.SaveChangesAsync();
    
        // Arrange
        var userService = new UserService(
            testFixture.GetService<IContextFactory>(),
            testFixture.GetService<IPasswordHasher>(),
            null,
            null,
            testFixture.GetService<IOptions<JwtToken>>(), 
            new OptionsWrapper<NotificationOptions>(null),
            null);
    
        var jwtToken = await userService.AuthenticateAsync(new AuthUserDto(email, password));
        
        // Assert
        Assert.False(string.IsNullOrWhiteSpace(jwtToken));
    }
    
    [Fact]
    public async Task AuthenticateAsync_InvalidPassword_UserExists_ShouldThrowExceptionWithStatusCodeUnauthorized()
    {
        // Act
        using var testFixture = new TestFixture();
    
        var email = "test@bobr.ru";
        var password = "password1";
        var invalidPassword = "password2";
        
        var dataContext = testFixture.GetService<DataContext>();
        dataContext.Users.Add(new User
        {
            Email = email,
            Password = testFixture.GetService<IPasswordHasher>().HashPassword(password),
            FirstName = string.Empty,
            LastName = string.Empty
        });
    
        // Arrange
        var userService = new UserService(
            testFixture.GetService<IContextFactory>(),
            testFixture.GetService<IPasswordHasher>(),
            null,
            null,
            new OptionsWrapper<JwtToken>(null),
            new OptionsWrapper<NotificationOptions>(null),
            Mock.Of<ILogger<UserService>>());
        
        // Assert
        await Assert.ThrowsAsync<InvalidAuthorizationException>(async () =>
        {
            var authUserDtoWithInvalidPassword = new AuthUserDto(email, invalidPassword);
    
            await userService.AuthenticateAsync(authUserDtoWithInvalidPassword);
        });
    }
    
    [Fact]
    public async Task AuthenticateAsync_UserNotExists_ShouldThrowExceptionWithStatusCodeUnauthorized()
    {
        // Act
        using var testFixture = new TestFixture();

        var email = "test@bobr.ru";
        var password = "password1";

        // Arrange
        var userService = new UserService(
            testFixture.GetService<IContextFactory>(),
            testFixture.GetService<IPasswordHasher>(),
            null,
            null,
            new OptionsWrapper<JwtToken>(null),
            new OptionsWrapper<NotificationOptions>(null),
            Mock.Of<ILogger<UserService>>());
        
        // Assert
        await Assert.ThrowsAsync<InvalidAuthorizationException>(async () =>
        {
            var authUserDtoWithInvalidPassword = new AuthUserDto(email, password);

            await userService.AuthenticateAsync(authUserDtoWithInvalidPassword);
        });
    }
    
   [Fact]
    public void ValidateToken_ValidToken_ReturnsTrue()
    {
        // Arrange
        var validToken = JwtHelper.GenerateJwtToken(_testSecretKey, 1, string.Empty);
        
        var jwtToken = new JwtToken { JwtSecret = _testSecretKey };
        var authService = new UserService(
            null, 
            null,
            null, 
            null, 
            new OptionsWrapper<JwtToken>(jwtToken),
            new OptionsWrapper<NotificationOptions>(null),
            null);

        // Act
        var result = authService.ValidateToken(validToken);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "wrong-secret-key";
        
        var jwtToken = new JwtToken { JwtSecret = _testSecretKey };
        var authService = new UserService(
            null, 
            null,
            null, 
            null, 
            new OptionsWrapper<JwtToken>(jwtToken),
            new OptionsWrapper<NotificationOptions>(null),
            null);

        // Act
        var result = authService.ValidateToken(invalidToken);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateToken_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var expiredToken = JwtHelper.GenerateJwtToken(_testSecretKey, 1, string.Empty, expires: DateTime.UtcNow.AddSeconds(2));

        await Task.Delay(TimeSpan.FromSeconds(5));
        
        var jwtToken = new JwtToken { JwtSecret = _testSecretKey };
        var authService = new UserService(
            null, 
            null,
            null, 
            null, 
            new OptionsWrapper<JwtToken>(jwtToken),
            new OptionsWrapper<NotificationOptions>(new NotificationOptions
            {
                Token = string.Empty
            }),
            null);

        // Act
        var result = authService.ValidateToken(expiredToken);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateToken_MalformedToken_ReturnsFalse()
    {
        // Arrange
        var malformedToken = "invalid-token";
        
        var jwtToken = new JwtToken { JwtSecret = _testSecretKey };
        var authService = new UserService(
            null, 
            null,
            null, 
            null,
            new OptionsWrapper<JwtToken>(jwtToken),
            new OptionsWrapper<NotificationOptions>(null),
            null);

        // Act
        var result = authService.ValidateToken(malformedToken);

        // Assert
        Assert.False(result);
    }
}