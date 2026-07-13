using Microsoft.EntityFrameworkCore;
using Student_Management_System.Models;
using Student_Management_System.Repositories.Interfaces;

namespace Student_Management_System.Repositories;

public class ParentRepository : IParentRepository
{
    private readonly AppDbContext _context;

    public ParentRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Parent?> FindByPhoneAsync(string phone)
    {
        return _context.Parents.FirstOrDefaultAsync(parent => !parent.IsDeleted && parent.Phone == phone);
    }

    public void Add(Parent parent)
    {
        _context.Parents.Add(parent);
    }
}
