using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using QuMail.EmailProtocol.Controllers;
using QuMail.EmailProtocol.Data;
using QuMail.EmailProtocol.Models;

namespace QuMail.EmailProtocol.Tests;

/// <summary>
/// Unit tests for AuthController
/// Tests authentication, registration, and JWT handling
/// </summary>
public class AuthControllerTests : IDisposable
{
    private readonly AuthDbContext _context;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly IConfiguration _configuration;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AuthDbContext(options);

        // Setup mock logger
        _loggerMock = new Mock<ILogger<AuthController>>();

        // Setup configuration
        var configValues = new Dictionary<string, string?>
        {
            {"JwtSettings:SecretKey", "super-secret-key-for-testing-that-is-at-least-32-chars"},
            {"JwtSettings:Issuer", "QuMail-Test"},
            {"JwtSettings:Audience", "QuMail-Test-Users"},
            {"JwtSettings:ExpiresInMinutes", "60"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        _controller = new AuthController(_context, _configuration, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region Login Tests

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = passwordHash,
            Name = "Test User",
            IsActive = true,
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var authResponse = okResult.Value.Should().BeOfType<AuthResponse>().Subject;
        authResponse.Token.Should().NotBeNullOrEmpty();
        authResponse.User.Should().NotBeNull();
        authResponse.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = passwordHash,
            Name = "Test User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_NonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_InactiveUser_ReturnsUnauthorized()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "inactive@example.com",
            Username = "inactiveuser",
            PasswordHash = passwordHash,
            Name = "Inactive User",
            IsActive = false, // Inactive user
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "inactive@example.com",
            Password = "password123"
        };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_UpdatesLastLoginAt()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var originalLoginTime = DateTime.UtcNow.AddDays(-1);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "login-time@example.com",
            Username = "logintime",
            PasswordHash = passwordHash,
            Name = "Test User",
            IsActive = true,
            LastLoginAt = originalLoginTime,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "login-time@example.com",
            Password = "password123"
        };

        // Act
        await _controller.Login(loginRequest);

        // Assert
        var updatedUser = await _context.Users.FirstAsync(u => u.Email == "login-time@example.com");
        updatedUser.LastLoginAt.Should().BeAfter(originalLoginTime);
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com",
            Username = "existing",
            PasswordHash = "hash",
            Name = "Existing User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(existingUser);
        await _context.SaveChangesAsync();

        var registerRequest = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "password123",
            Name = "New User",
            ExternalEmail = "existing@example.com",
            EmailProvider = "gmail",
            AppPassword = "1234567890123456"
        };

        // Act
        var result = await _controller.Register(registerRequest);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region JWT Token Tests

    [Fact]
    public async Task Login_GeneratedToken_ContainsUserClaims()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "claims@example.com",
            Username = "claimsuser",
            PasswordHash = passwordHash,
            Name = "Claims User",
            IsActive = true,
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "claims@example.com",
            Password = "password123"
        };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var authResponse = okResult.Value.Should().BeOfType<AuthResponse>().Subject;

        // Token should be a valid JWT (3 parts separated by dots)
        var tokenParts = authResponse.Token.Split('.');
        tokenParts.Should().HaveCount(3);
    }

    #endregion

    #region Password Hashing Tests

    [Fact]
    public void BCryptHashing_ValidPassword_VerifiesCorrectly()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var isValid = BCrypt.Net.BCrypt.Verify(password, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void BCryptHashing_DifferentPasswords_ProduceDifferentHashes()
    {
        // Arrange
        var password1 = "Password1";
        var password2 = "Password2";

        // Act
        var hash1 = BCrypt.Net.BCrypt.HashPassword(password1);
        var hash2 = BCrypt.Net.BCrypt.HashPassword(password2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void BCryptHashing_SamePassword_ProducesDifferentHashes()
    {
        // Arrange - BCrypt should produce different hashes due to salt
        var password = "SamePassword";

        // Act
        var hash1 = BCrypt.Net.BCrypt.HashPassword(password);
        var hash2 = BCrypt.Net.BCrypt.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2);
        BCrypt.Net.BCrypt.Verify(password, hash1).Should().BeTrue();
        BCrypt.Net.BCrypt.Verify(password, hash2).Should().BeTrue();
    }

    [Fact]
    public void BCryptHashing_WrongPassword_DoesNotVerify()
    {
        // Arrange
        var correctPassword = "CorrectPassword";
        var wrongPassword = "WrongPassword";

        // Act
        var hash = BCrypt.Net.BCrypt.HashPassword(correctPassword);
        var isValid = BCrypt.Net.BCrypt.Verify(wrongPassword, hash);

        // Assert
        isValid.Should().BeFalse();
    }

    #endregion
}
