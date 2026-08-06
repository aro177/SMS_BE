using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Student_Management_System.Config;
using Student_Management_System.Configs;
using Student_Management_System.Configs.HttpContext;
using Student_Management_System.Integrations.supabase;
using Student_Management_System.Integrations.turnstile;
using Student_Management_System.Models;
using Student_Management_System.Models.Enum;
using Student_Management_System.Repositories;
using Student_Management_System.Repositories.Interfaces;
using Student_Management_System.Services;
using Student_Management_System.Services.Interfaces;
using System.Data;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

LoadDockerSecret("ConnectionStrings:DefaultConnection", "db_connection");
LoadDockerSecret("SUPABASE_KEY", "supabase_key");
LoadDockerSecret("Supabase:ApiSecretKey", "supabase_api_secret_key");
LoadDockerSecret("Turnstile:SecretKey", "turnstile_secret_key");

var allowedOrigins = (builder.Configuration["Cors:AllowedOrigins"] ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(origin => origin.TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

if (allowedOrigins.Length == 0 && builder.Environment.IsDevelopment())
{
    allowedOrigins = ["http://localhost:3000", "http://127.0.0.1:3000"];
}

if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("Cors:AllowedOrigins must be configured in non-development environments.");
}

void LoadDockerSecret(string configurationKey, string secretName)
{
    var secretPath = Path.Combine("/run/secrets", secretName);
    if (File.Exists(secretPath))
    {
        builder.Configuration[configurationKey] = File.ReadAllText(secretPath).Trim();
    }
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;

    var knownProxyValue = builder.Configuration["ReverseProxy:KnownProxy"];
    if (IPAddress.TryParse(knownProxyValue, out var knownProxy))
    {
        options.KnownProxies.Add(knownProxy);
    }
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add services to the container.
var url = builder.Configuration["SUPABASE_URL"];
var key = builder.Configuration["SUPABASE_KEY"];
var options = new Supabase.SupabaseOptions
{
    AutoConnectRealtime = true
};
var supabase = new Supabase.Client(url, key, options);
await supabase.InitializeAsync();

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("StudentSearch", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Authentication:ValidIssuer"];
    options.Audience = builder.Configuration["Authentication:ValidAudience"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = true,
        ValidateIssuer = true,
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var identity = (ClaimsIdentity)context.Principal!.Identity!;

            var claims = context.Principal!;

            var userId = Guid.Parse(
                claims.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? claims.FindFirst("sub")!.Value);

            var email = claims.FindFirst(ClaimTypes.Email)?.Value
                        ?? claims.FindFirst("email")?.Value;

            string? roleString = null;

                var appMetadata =
                    claims.FindFirst("app_metadata")?.Value;

                if (!string.IsNullOrEmpty(appMetadata))
                {
                    using var doc = JsonDocument.Parse(appMetadata);

                    if (doc.RootElement.TryGetProperty("role", out var role))
                    {
                        roleString = role.GetString();
                    }
                }

            if(!string.IsNullOrEmpty(roleString))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, roleString));
            }

            if (!Enum.TryParse<Role>(roleString, out var roleEnum))
            {
                context.Fail("Role is not valid.");
                return Task.CompletedTask;
            }

            context.HttpContext.Items["CurrentUser"] = new CurrentUser
            {
                UserId = userId,
                Email = email ?? "",
                Role = roleEnum
            };

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), o =>
    {
        o.MapEnum<AttendanceStatus>("attendance_status");
        o.MapEnum<EnrollmentStatus>("enrollment_status");
        o.MapEnum<RepeatStatus>("repeat_status");
        o.CommandTimeout(120);
        o.EnableRetryOnFailure();
    });
});

builder.Services.Configure<SupabaseOptions>(
    builder.Configuration.GetSection("Supabase"));
builder.Services.Configure<TurnstileOptions>(
    builder.Configuration.GetSection("Turnstile"));

builder.Services.AddTransient<SupabaseAuthHandler>();

builder.Services.AddHttpClient<ISupabaseAuthClient, SupabaseAuthClient>()
    .AddHttpMessageHandler<SupabaseAuthHandler>();
builder.Services.AddHttpClient<ITurnstileVerificationService, CloudflareTurnstileVerificationService>(client =>
{
    client.BaseAddress = new Uri("https://challenges.cloudflare.com/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IClassroomRepository, ClassroomRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IParentRepository, ParentRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();

builder.Services.AddScoped<IClassRegistrationService, ClassRegistrationService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IClassroomService, ClassroomService>();
builder.Services.AddScoped<IDatabaseMaintenanceService, DatabaseMaintenanceService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseForwardedHeaders();

app.UseRateLimiter();

app.UseCors("Frontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
