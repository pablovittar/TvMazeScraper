
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using TvMazeScraper.SyncTasks;
using TvMazeScraper.Infrastructure;
using TvMazeScraper.SyncTasks.Workers;

namespace TvMazeScraper.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = GetConfiguration();
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders().AddConsole();

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddOptions<TvMazeSyncSettings>().Bind(configuration.GetSection("TvMazeSyncSettings"));
            builder.Services.AddDbContext<TvMazeContext>(options =>
            {
                options.UseSqlite($"Data Source={Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TvMazeScraper.db")}");
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(AddSwaggerDocumentation);
            builder.Services.AddHostedService<TvMazeSyncWorker>();

            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TvMazeContext>();
                context.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
        public static void AddSwaggerDocumentation(SwaggerGenOptions o)
        {
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        }

        static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            return builder.Build();
        }
    }
}