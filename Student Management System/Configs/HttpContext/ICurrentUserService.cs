namespace Student_Management_System.Configs.HttpContext
{
    public interface ICurrentUserService
    {
        CurrentUser? User { get; }
    }
}
