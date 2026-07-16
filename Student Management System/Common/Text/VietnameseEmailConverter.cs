using System.Globalization;
using System.Text;

namespace Student_Management_System.Common.Text;

public static class VietnameseEmailConverter
{
    public static string ToMailAddress(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        var normalized = fullName.Trim().ToLowerInvariant()
            .Replace('đ', 'd')
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder();
        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
        }

        if (builder.Length == 0)
        {
            throw new ArgumentException("Full name must contain at least one letter or digit.", nameof(fullName));
        }

        return $"{builder}@mail.com";
    }
}
