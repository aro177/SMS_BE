using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Student_Management_System.Config;
using Student_Management_System.Configs.HttpContext;
using Student_Management_System.Integrations.supabase;
using Student_Management_System.Models;
using Student_Management_System.Models.Enum;
using Student_Management_System.Repositories;
using Student_Management_System.Repositories.Interfaces;
using Student_Management_System.Services;
using Student_Management_System.Services.Interfaces;
using System.Data;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

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
    });
});

builder.Services.Configure<SupabaseOptions>(
    builder.Configuration.GetSection("Supabase"));

builder.Services.AddTransient<SupabaseAuthHandler>();

builder.Services.AddHttpClient<ISupabaseAuthClient, SupabaseAuthClient>()
    .AddHttpMessageHandler<SupabaseAuthHandler>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IClassroomRepository, ClassroomRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IParentRepository, ParentRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();

builder.Services.AddScoped<IClassRegistrationService, ClassRegistrationService>();
builder.Services.AddScoped<IClassroomService, ClassroomService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
