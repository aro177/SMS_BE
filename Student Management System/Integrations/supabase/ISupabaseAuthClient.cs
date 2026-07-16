using Student_Management_System.Models.Enum;
using System.Data;

namespace Student_Management_System.Integrations.supabase
{
    public interface ISupabaseAuthClient
    {
        string BuildEmailFromFullName(string fullName);

        Task<Guid> CreateAccountAsync(
            Role role,
            string email,
            string password,
            string phone);

        Task ChangePasswordAsync(Guid accountId, string password);

        Task<bool> CheckOldPasswordAsync(
            string email,
            string password);
    }
}
