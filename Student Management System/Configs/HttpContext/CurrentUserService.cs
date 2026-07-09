namespace Student_Management_System.Configs.HttpContext
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor accessor)
        {
            _httpContextAccessor = accessor;
        }

        public CurrentUser? User =>
            _httpContextAccessor.HttpContext?
                .Items["CurrentUser"] as CurrentUser;
    }
}
