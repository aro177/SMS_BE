using Microsoft.EntityFrameworkCore;
using Student_Management_System.Common.Pagination;
using Student_Management_System.Configs.HttpContext;
using Student_Management_System.Dtos.Teachers;
using Student_Management_System.Integrations.supabase;
using Student_Management_System.Models;
using Student_Management_System.Models.Enum;
using Student_Management_System.Repositories.Interfaces;
using Student_Management_System.Services.Interfaces;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Student_Management_System.Services;

public class TeacherService : ITeacherService
{
    private readonly ICurrentUserService _currentUser;
    private readonly ITeacherRepository _teachers;
    private readonly ISupabaseAuthClient _authClient;

    public TeacherService(ICurrentUserService currentUser, ITeacherRepository teachers, ISupabaseAuthClient authClient)
    {
        _currentUser = currentUser;
        _teachers = teachers;
        _authClient = authClient;
    }

    public Task<PagedResult<TeacherResponse>> GetPagedAsync(PaginationQuery pagination)
    {
        return _teachers.GetPagedAsync(pagination);
    }

    public async Task<TeacherResponse?> GetCurrentTeacherAsync()
    {
        var userId = _currentUser.User?.UserId;
        if (userId is null)
        {
            return null;
        }

        var teacher = await _teachers.GetActiveByAuthUserIdAsync(userId.Value);
        return teacher is null
            ? null
            : new TeacherResponse(
                teacher.Id,
                teacher.Fullname,
                teacher.Phone,
                teacher.Classrooms.Count(classroom => !classroom.IsDeleted),
                teacher.AuthUserId);
    }

    public async Task<TeacherResponse> CreateAsync(CreateTeacherRequest request)
    {
        var accountEmail = _authClient.BuildEmailFromFullName(request.Fullname ?? "");

        Guid authId = await _authClient.CreateAccountAsync(
            Role.TEACHER,
            accountEmail,
            string.IsNullOrWhiteSpace(request.Phone) ? "12345678" : request.Phone.Trim(),
            string.IsNullOrWhiteSpace(request.Phone) ? "0" : request.Phone.Trim());

        string code = await GenerateUniqueCodeAsync(request.Fullname ?? "");

        var teacher = new Teacher
        {
            Fullname = request.Fullname.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            AuthUserId = authId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Code = code
        };

        _teachers.Add(teacher);
        await _teachers.SaveChangesAsync();

        return new TeacherResponse(teacher.Id, teacher.Fullname, teacher.Phone, 0, authId);
    }

    public async Task<bool> UpdateAsync(long id, UpdateTeacherRequest request)
    {
        var teacher = await _teachers.GetActiveByIdAsync(id);
        if (teacher is null)
        {
            return false;
        }

        teacher.Fullname = request.Fullname.Trim();
        teacher.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        teacher.AuthUserId = request.AuthUserId;
        teacher.UpdatedAt = DateTime.UtcNow;

        await _teachers.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var teacher = await _teachers.GetActiveByIdAsync(id);
        if (teacher is null)
        {
            return false;
        }

        teacher.IsDeleted = true;
        teacher.UpdatedAt = DateTime.UtcNow;

        await _teachers.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerateUniqueCodeAsync(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException(
                "Tên không được để trống.",
                nameof(fullName)
            );
        }

        string normalizedFullName = Regex.Replace(
            fullName.Trim(),
            @"\s+",
            " "
        );

        string[] nameParts = normalizedFullName.Split(' ');

        if (nameParts.Length < 2)
        {
            throw new ArgumentException(
                "Tên đầy đủ phải có ít nhất họ và tên.",
                nameof(fullName)
            );
        }

        string firstName = RemoveVietnameseDiacritics(nameParts[^1]);

        var initialsBuilder = new StringBuilder();

        for (int i = 0; i < nameParts.Length - 1; i++)
        {
            string partWithoutDiacritics =
                RemoveVietnameseDiacritics(nameParts[i]);

            if (!string.IsNullOrWhiteSpace(partWithoutDiacritics))
            {
                initialsBuilder.Append(
                    char.ToUpperInvariant(partWithoutDiacritics[0])
                );
            }
        }

        string initials = initialsBuilder.ToString();

        firstName = ToPascalCase(firstName);

        string prefix = $"{firstName}{initials}";

        List<string?> existingUsernames = await _teachers.GetExistingCodeAsync(prefix);

        int latestIndex = existingUsernames
            .Select(username => ExtractIndex(username, prefix))
            .DefaultIfEmpty(0)
            .Max();

        int nextIndex = latestIndex + 1;

        return $"{prefix}{nextIndex:D2}";
    }

    private static int ExtractIndex(string username, string prefix)
    {
        if (string.IsNullOrWhiteSpace(username) ||
            !username.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase
            ))
        {
            return 0;
        }

        string indexPart = username[prefix.Length..];

        return int.TryParse(indexPart, out int index)
            ? index
            : 0;
    }

    private static string ToPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        value = value.ToLowerInvariant();

        return char.ToUpperInvariant(value[0]) + value[1..];
    }

    private static string RemoveVietnameseDiacritics(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string normalized = value.Normalize(
            NormalizationForm.FormD
        );

        var result = new StringBuilder();

        foreach (char character in normalized)
        {
            UnicodeCategory category =
                CharUnicodeInfo.GetUnicodeCategory(character);

            if (category != UnicodeCategory.NonSpacingMark)
            {
                result.Append(character);
            }
        }

        return result
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd')
            .Replace('Đ', 'D');
    }

}
