using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Roshetta.API.Data;
using Roshetta.API.Middlewares;
using Roshetta.API.Models;
using Roshetta.API.Models.Validators;
using Roshetta.API.Repositories;
using Roshetta.API.Services;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Text;
using System.Threading.RateLimiting;

namespace Roshetta.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Connect Serilog to ASP.NET Core logging
                builder.Host.UseSerilog((context, services, config) => config
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services));

                // Add services to the container.

                builder.Services.AddControllers();

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Description = "Paste the access token only (no 'Bearer ' prefix)."
                    });

                    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    });
                });

                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString));

                builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                })
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = builder.Configuration["Jwt:Issuer"],
                            ValidAudience = builder.Configuration["Jwt:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration.")))
                        };
                    });

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

                builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();  
                builder.Services.AddScoped<IDoctorService, DoctorService>();
                builder.Services.AddScoped<ISpecializationRepository, SpecializationRepository>();
                builder.Services.AddScoped<ISpecializationService, SpecializationService>();

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
                app.UseRateLimiter(); // Rate Limiting Middleware   

                app.UseAuthentication(); // Authentication Middleware
                app.UseAuthorization();


                app.MapControllers();

                app.Run();
            }
            catch (Exception ex) when (ex is not HostAbortedException) // The catch-all try/catch will log a fatal error every time you run Add-Migration. EF’s tooling stops the host on purpose by throwing HostAbortedException
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
