using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Integrations.supabase;
using Student_Management_System.Models;
using Student_Management_System.Models.Enum;

namespace Student_Management_System.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISupabaseAuthClient _authClient;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, AppDbContext context, ISupabaseAuthClient authClient)
        {
            _logger = logger;
            _context = context;
            _authClient = authClient;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("student")]
        [Authorize(Roles = "TEACHER")]
        public IActionResult GetStudents()
        {
            try
            {
                var students = _context.Students.ToList();
                return Ok(students);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet("create-account")]
        public async Task<IActionResult> CreateAccount(
        [FromQuery] string email,
        [FromQuery] Role role)
        {
            await _authClient.CreateAccountAsync(
                role,
                email,
                "12345678",
                "0123456789");

            return Ok("OK");
        }
    }
}
