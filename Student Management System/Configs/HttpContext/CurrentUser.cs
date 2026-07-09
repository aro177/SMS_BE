using Student_Management_System.Models.Enum;

namespace Student_Management_System.Configs.HttpContext
{
    public class CurrentUser
    {
        public Guid UserId { get; init; }

        public string Email { get; init; } = string.Empty;

        public Role Role { get; init; }
    }
}
