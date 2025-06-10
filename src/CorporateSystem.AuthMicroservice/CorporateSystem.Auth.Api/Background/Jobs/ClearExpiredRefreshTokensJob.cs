using CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace CorporateSystem.Auth.Api.Background.Jobs;

public class ClearExpiredRefreshTokensJob(IContextFactory contextFactory, ILogger<ClearExpiredRefreshTokensJob> logger) 
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTimeOffset.UtcNow;

        await contextFactory.ExecuteWithCommitAsync(async dataContext =>
        {
            var expiredRefreshTokens = await dataContext.RefreshTokens
                .Where(token => token.ExpiryOn < now)
                .ToArrayAsync();

            if (expiredRefreshTokens.Any())
            {
                dataContext.RefreshTokens.RemoveRange(expiredRefreshTokens);
                logger.LogInformation($"Job {nameof(ClearExpiredRefreshTokensJob)}: Deleted {expiredRefreshTokens.Length} refresh tokens");
            }
        });
    }
}