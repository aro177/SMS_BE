using Microsoft.Extensions.Options;
using Student_Management_System.Config;
using Student_Management_System.Integrations.supabase.dto;
using Student_Management_System.Models;
using Student_Management_System.Models.Enum;
using Supabase.Gotrue;

namespace Student_Management_System.Integrations.supabase
{
    public class SupabaseAuthClient : ISupabaseAuthClient
    {
        private readonly HttpClient _httpClient;
        private readonly SupabaseOptions _options;
        private readonly AppDbContext _db;

        public SupabaseAuthClient(
            HttpClient httpClient,
            IOptions<SupabaseOptions> options,
            AppDbContext db)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _db = db;
        }

        public Task ChangePasswordAsync(Guid accountId, string password)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckOldPasswordAsync(string email, string password)
        {
            throw new NotImplementedException();
        }

        public async Task<Guid> CreateAccountAsync(Role role, string email, string password, string phone)
        {
            var request = new
            {
                email,
                phone = ToE164(phone),
                password,
                email_confirm = true,
                app_metadata = new
                {
                    role = role.ToString()
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_options.Url}/auth/v1/admin/users",
                request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            var user =
                await response.Content.ReadFromJsonAsync<UserResponse>();

            return user.Id;
        }

        private string ToE164(string phone)
        {
            if (phone.StartsWith("0"))
                return "+84" + phone[1..];

            return phone;
        }
    }
}
