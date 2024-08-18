using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserPasswordUpdateJob.Data;

namespace UserPasswordUpdateJob
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    var smtpServer = context.Configuration["Smtp:Server"];
                    var smtpPort = int.Parse(context.Configuration["Smtp:Port"]);
                    var smtpUsername = context.Configuration["Smtp:Username"];
                    var smtpPassword = context.Configuration["Smtp:Password"];
                    var fromEmail = context.Configuration["Smtp:FromEmail"];

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

                    services.AddSingleton<IEmailService>(provider =>
                        new EmailService(smtpServer, smtpPort, smtpUsername, smtpPassword, fromEmail));

                    services.AddHostedService<Worker>();
                });
    }
}
