using CorporateSystem.Auth.Api.Background.Jobs;
using CorporateSystem.Auth.Domain.Entities;
using CorporateSystem.Auth.Infrastructure;
using CorporateSystem.Auth.Infrastructure.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CorporateSystem.Auth.Tests.IntegrationTests.JobTests;

public class ClearExpiredRefreshTokensJobTests
{
    [Fact]
    public async Task Execute_ShouldRemoveExpiredTokens()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        await using var context = new DataContext(options);
        context.RefreshTokens.AddRange(
            new RefreshToken
            {
                Token = "valid-token",
                ExpiryOn = DateTimeOffset.UtcNow.AddDays(1), 
                UserId = 1,
                User = null,
                IpAddress = string.Empty
            },
            new RefreshToken
            {
                Token = "expired-token",
                ExpiryOn = DateTimeOffset.UtcNow.AddDays(-1),
                UserId = 2,
                User = null,
                IpAddress = string.Empty
            }
        );
        
        await context.SaveChangesAsync();

        var contextFactory = new ContextFactory<DataContext>(options);
        
        // Act
        var job = new ClearExpiredRefreshTokensJob(contextFactory, Mock.Of<ILogger<ClearExpiredRefreshTokensJob>>());
        await job.Execute(null);
        
        var remainingTokens = await context.RefreshTokens.ToListAsync();

        // Assert
        Assert.Single(remainingTokens);
        Assert.Equal("valid-token", remainingTokens[0].Token);
    }
}