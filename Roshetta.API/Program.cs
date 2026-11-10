using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Roshetta.API.Data;
using Roshetta.API.Middlewares;
using Roshetta.API.Models;
using Roshetta.API.Models.Validators;
using Roshetta.API.Services;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Threading.RateLimiting;

namespace Roshetta.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/roshetta-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
                .CreateBootstrapLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Connect Serilog to ASP.NET Core logging
                builder.Host.UseSerilog((context, services, config) => config
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File(
                        Path.Combine(AppContext.BaseDirectory, "logs", "roshetta-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 14));

                // Add services to the container.

                builder.Services.AddControllers();

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString));

                builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                })
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

                // Fluent Validation For Register DTO
                builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

                // Auto Validation for Fluent Validation
                builder.Services.AddFluentValidationAutoValidation();

                builder.Services.AddScoped<IAuthService, AuthService>();

                builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
                builder.Services.AddProblemDetails();

                builder.Services.AddRateLimiter(options =>
                {
                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                    options.AddPolicy("auth", context =>
                        RateLimitPartition.GetSlidingWindowLimiter(
                            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                            factory: _ => new SlidingWindowRateLimiterOptions
                            {
                                PermitLimit = 5,
                                Window = TimeSpan.FromMinutes(1),
                                SegmentsPerWindow = 6,   // 10-second segments
                                QueueLimit = 0           // reject immediately, don't queue
                            }
                        )
                    );

                    options.OnRejected = async (context, ct) =>
                    {
                        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("Rate limit hit on {Path} from {Ip}",
                            context.HttpContext.Request.Path,
                            context.HttpContext.Connection.RemoteIpAddress);

                        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
                        {
                            Status = StatusCodes.Status429TooManyRequests,
                            Title = "Too many requests",
                            Detail = "Please try again later."
                        }, ct);
                    };

                });

                var app = builder.Build();

                // To Seed The Database with Initial Data and Add an Admin Account 
                using (var scope = app.Services.CreateScope())
                {
                    await DbSeeder.SeedAsync(scope.ServiceProvider);
                }

                app.UseExceptionHandler();        // makes GlobalExceptionHandler actually run
                app.UseSerilogRequestLogging();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();

                app.UseAuthorization();

                app.UseRateLimiter(); // Rate Limiting Middleware   

                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
