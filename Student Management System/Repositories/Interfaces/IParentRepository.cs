using Student_Management_System.Models;

namespace Student_Management_System.Repositories.Interfaces;

public interface IParentRepository
{
    Task<Parent?> FindByPhoneAsync(string phone);
    void Add(Parent parent);
}
