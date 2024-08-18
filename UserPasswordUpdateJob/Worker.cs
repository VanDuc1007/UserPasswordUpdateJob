using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserPasswordUpdateJob.Data;

public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(IServiceScopeFactory scopeFactory, ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
                var users = dbContext.Users.Where(u => u.LastUpdatePwd < sixMonthsAgo && u.Status != "REQUIRE_CHANGE_PWD").ToList();

                foreach (var user in users)
                {
                    user.Status = "REQUIRE_CHANGE_PWD";
                }

                await dbContext.SaveChangesAsync(stoppingToken);

                foreach (var user in users)
                {
                    await emailService.SendEmailAsync(user.Email, "Password Update Required", "Your password needs to be updated.");
                }

                _logger.LogInformation("Password update check completed.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // Delay 24 hours before next check
        }
    }
}
