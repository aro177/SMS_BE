using Student_Management_System.Dtos.ClassRegistrations;

namespace Student_Management_System.Services.Interfaces;

public interface IClassRegistrationService
{
    Task<ClassRegistrationResponse?> RegisterAsync(ClassRegistrationRequest request);
}
