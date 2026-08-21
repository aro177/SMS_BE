using Student_Management_System.Common.DateTimes;
using Student_Management_System.Common.Pagination;
using Student_Management_System.Configs.HttpContext;
using Student_Management_System.Dtos.Lessons;
using Student_Management_System.Models;
using Student_Management_System.Models.Enum;
using Student_Management_System.Repositories.Interfaces;
using Student_Management_System.Services.Interfaces;
using System.Globalization;
using System.Text;

namespace Student_Management_System.Services;

public class LessonService : ILessonService
{
    private const int MaxGeneratedOccurrences = 500;
    private const int MaxNeverWeeks = 20;

    private readonly IClassroomRepository _classrooms;
    private readonly ICurrentUserService _currentUser;
    private readonly ILessonRepository _lessons;
    private readonly ITeacherRepository _teachers;

    public LessonService(
        IClassroomRepository classrooms,
        ICurrentUserService currentUser,
        ILessonRepository lessons,
        ITeacherRepository teachers)
    {
        _classrooms = classrooms;
        _currentUser = currentUser;
        _lessons = lessons;
        _teachers = teachers;
    }

    public Task<PagedResult<LessonResponse>> GetPagedAsync(LessonFilter filter, PaginationQuery pagination)
    {
        var normalizedFilter = new LessonFilter(
            DateTimeUtc.Normalize(filter.From),
            DateTimeUtc.Normalize(filter.To),
            filter.TeacherId,
            filter.ClassroomId);

        return _lessons.GetPagedAsync(normalizedFilter, pagination);
    }

    public async Task<IReadOnlyList<LessonAttendanceResponse>?> GetAttendancesAsync(long lessonId)
    {
        var lesson = await _lessons.GetActiveByIdAsync(lessonId);
        return lesson is null ? null : await _lessons.GetAttendancesAsync(lessonId);
    }

    public async Task<IReadOnlyList<LessonResponse>?> GetTodayForCurrentTeacherAsync(DateOnly? date = null)
    {
        var userId = _currentUser.User?.UserId;
        if (userId is null)
        {
            return null;
        }

        var teacher = await _teachers.GetActiveByAuthUserIdAsync(userId.Value);
        if (teacher is null)
        {
            return null;
        }

        var targetDate = date ?? DateTimeUtc.TodayInVietnam();
        var start = DateTimeUtc.FromVietnamLocal(targetDate, TimeOnly.MinValue);
        var end = DateTimeUtc.FromVietnamLocal(targetDate, TimeOnly.MaxValue);
        var lessons = await _lessons.GetPagedAsync(new LessonFilter(start, end, teacher.Id, null), new PaginationQuery { Page = 1, PageSize = 100 });

        return lessons.Items;
    }

    public Task<IReadOnlyList<LessonResponse>> GetTodayAsync(DateOnly? date = null)
    {
        var targetDate = date ?? DateTimeUtc.TodayInVietnam();
        var start = DateTimeUtc.FromVietnamLocal(targetDate, TimeOnly.MinValue);
        var endExclusive = DateTimeUtc.FromVietnamLocal(targetDate.AddDays(1), TimeOnly.MinValue);

        return _lessons.GetByStartRangeAsync(start, endExclusive);
    }

    public async Task<CreateLessonsResponse?> CreateAsync(CreateLessonRequest request)
    {
        var startTime = request.StartTime.UtcDateTime;
        var endTime = request.EndTime.UtcDateTime;
        var classroom = await _classrooms.GetActiveByIdAsync(request.ClassroomId);
        if (classroom is null || endTime <= startTime || !Enum.IsDefined(request.RepeatStatus))
        {
            return null;
        }

        var occurrenceStartTimes = BuildOccurrenceStartTimes(request, startTime);
        if (occurrenceStartTimes is null || occurrenceStartTimes.Count == 0)
        {
            return null;
        }

        var duration = endTime - startTime;
        var now = DateTime.UtcNow;
        var title = string.IsNullOrWhiteSpace(request.Title) ? classroom.Name : request.Title.Trim();
        var lessons = occurrenceStartTimes.Select(occurrenceStartTime => new Lesson
        {
            ClassroomId = request.ClassroomId,
            Title = title,
            StartTime = occurrenceStartTime,
            EndTime = occurrenceStartTime.Add(duration),
            RepeatStatus = request.RepeatStatus,
            Code = GenerateCode(classroom.Name, occurrenceStartTime),
            CreatedAt = now,
            UpdatedAt = now
        }).ToList();

        _lessons.AddRange(lessons);
        await _lessons.SaveChangesAsync();

        var responses = lessons.Select(lesson => new LessonResponse(
                lesson.Id,
                lesson.ClassroomId,
                classroom.Name,
                classroom.TeacherId,
                classroom.Teacher?.Fullname,
                lesson.Title,
                lesson.StartTime,
                lesson.EndTime,
                lesson.Code ?? string.Empty,
                lesson.TakeAttendanceStatus,
                lesson.RepeatStatus))
            .ToList();

        return new CreateLessonsResponse(responses, responses.Count);
    }

    public async Task<TakeAttendanceStatusResponse?> ToggleTakeAttendanceStatusAsync(long lessonId)
    {
        var lesson = await _lessons.GetActiveByIdAsync(lessonId);
        if (lesson is null)
        {
            return null;
        }

        lesson.TakeAttendanceStatus = !lesson.TakeAttendanceStatus;
        lesson.UpdatedAt = DateTime.UtcNow;

        await _lessons.SaveChangesAsync();
        return new TakeAttendanceStatusResponse(lesson.Id, lesson.TakeAttendanceStatus);
    }

    public async Task<BulkTakeAttendanceStatusResponse> ToggleTodayTakeAttendanceStatusAsync(
        DateOnly? date = null)
    {
        var targetDate = date ?? DateTimeUtc.TodayInVietnam();
        var start = DateTimeUtc.FromVietnamLocal(targetDate, TimeOnly.MinValue);
        var endExclusive = DateTimeUtc.FromVietnamLocal(targetDate.AddDays(1), TimeOnly.MinValue);
        var lessons = await _lessons.GetActiveEntitiesByStartRangeAsync(start, endExclusive);

        var takeAttendanceStatus = lessons.Count > 0 && lessons.Any(lesson => !lesson.TakeAttendanceStatus);
        var updatedAt = DateTime.UtcNow;

        foreach (var lesson in lessons)
        {
            lesson.TakeAttendanceStatus = takeAttendanceStatus;
            lesson.UpdatedAt = updatedAt;
        }

        if (lessons.Count > 0)
        {
            await _lessons.SaveChangesAsync();
        }

        return new BulkTakeAttendanceStatusResponse(
            targetDate,
            takeAttendanceStatus,
            lessons.Count);
    }

    public async Task<bool> UpdateAsync(long id, UpdateLessonRequest request)
    {
        var startTime = request.StartTime.UtcDateTime;
        var endTime = request.EndTime.UtcDateTime;
        var lesson = await _lessons.GetActiveByIdAsync(id);
        var classroom = await _classrooms.GetActiveByIdAsync(request.ClassroomId);
        if (lesson is null || classroom is null || endTime <= startTime || !Enum.IsDefined(request.RepeatStatus))
        {
            return false;
        }

        var startTimeChanged = lesson.StartTime != startTime;

        lesson.ClassroomId = request.ClassroomId;
        lesson.Title = request.Title.Trim();
        lesson.StartTime = startTime;
        lesson.EndTime = endTime;
        lesson.RepeatStatus = request.RepeatStatus;
        if (startTimeChanged)
        {
            lesson.Code = GenerateCode(classroom.Name, startTime);
        }
        lesson.UpdatedAt = DateTime.UtcNow;

        await _lessons.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var lesson = await _lessons.GetActiveByIdAsync(id);
        if (lesson is null)
        {
            return false;
        }

        lesson.IsDeleted = true;
        lesson.UpdatedAt = DateTime.UtcNow;

        await _lessons.SaveChangesAsync();
        return true;
    }

    private static IReadOnlyList<DateTime>? BuildOccurrenceStartTimes(
        CreateLessonRequest request,
        DateTime startTime)
    {
        if (request.RepeatStatus == RepeatStatus.TEMPORARY)
        {
            return [startTime];
        }

        var localStart = DateTimeUtc.ToVietnamLocal(startTime);
        var startDate = DateOnly.FromDateTime(localStart);
        var startTimeOfDay = TimeOnly.FromDateTime(localStart);
        var recurrence = request.Recurrence ?? new CustomLessonRecurrenceRequest(
            1,
            [localStart.DayOfWeek],
            RecurrenceEndType.Never,
            null,
            null);

        if (recurrence.IntervalWeeks is < 1 or > 20 ||
            recurrence.Weekdays is null ||
            recurrence.Weekdays.Count == 0 ||
            recurrence.Weekdays.Any(day => !Enum.IsDefined(day)) ||
            !Enum.IsDefined(recurrence.EndType))
        {
            return null;
        }

        var weekdays = recurrence.Weekdays.Distinct().ToHashSet();
        DateOnly? lastDate = recurrence.EndType == RecurrenceEndType.OnDate
            ? recurrence.EndDate
            : null;
        if (recurrence.EndType == RecurrenceEndType.Never)
        {
            var horizonDays = MaxNeverWeeks * 7 - 1;
            if (startDate.DayNumber > DateOnly.MaxValue.DayNumber - horizonDays)
            {
                return null;
            }

            lastDate = startDate.AddDays(horizonDays);
        }

        if (recurrence.EndType == RecurrenceEndType.OnDate &&
            (lastDate is null || lastDate < startDate))
        {
            return null;
        }

        var targetCount = recurrence.EndType == RecurrenceEndType.AfterOccurrences
            ? recurrence.OccurrenceCount
            : null;
        if (recurrence.EndType == RecurrenceEndType.AfterOccurrences &&
            (targetCount is null or < 1 or > MaxGeneratedOccurrences))
        {
            return null;
        }

        var anchorMonday = GetMonday(startDate);
        var occurrences = new List<DateTime>();
        for (var date = startDate; ;)
        {
            if (lastDate is not null && date > lastDate)
            {
                break;
            }

            var weekOffset = (GetMonday(date).DayNumber - anchorMonday.DayNumber) / 7;
            if (weekOffset % recurrence.IntervalWeeks == 0 && weekdays.Contains(date.DayOfWeek))
            {
                occurrences.Add(DateTimeUtc.FromVietnamLocal(date, startTimeOfDay));
                if (targetCount is not null && occurrences.Count == targetCount)
                {
                    break;
                }

                if (occurrences.Count > MaxGeneratedOccurrences)
                {
                    return null;
                }
            }

            if (lastDate is not null && date == lastDate)
            {
                break;
            }

            if (date == DateOnly.MaxValue)
            {
                return null;
            }

            date = date.AddDays(1);
        }

        return occurrences.Count == 0 ? null : occurrences;
    }

    private static DateOnly GetMonday(DateOnly date)
    {
        var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysSinceMonday);
    }

    private string GenerateCode(string classroomName, DateTime dateTime)
    {
        if (string.IsNullOrWhiteSpace(classroomName))
        {
            throw new ArgumentException(
                "Tên lớp không được để trống.",
                nameof(classroomName)
            );
        }

        string normalizedName = RemoveVietnameseDiacritics(
            classroomName.Trim()
        );

        char classroomInitial = normalizedName
            .FirstOrDefault(char.IsLetter);

        if (classroomInitial == default)
        {
            throw new ArgumentException(
                "Tên lớp phải chứa ít nhất một chữ cái.",
                nameof(classroomName)
            );
        }

        var vietnamDateTime = DateTimeUtc.ToVietnamLocal(dateTime);

        string dayCode = GetDayCode(vietnamDateTime.DayOfWeek);

        string timeCode = vietnamDateTime.ToString(
            "HHmm",
            CultureInfo.InvariantCulture
        );

        return $"{char.ToUpperInvariant(classroomInitial)}_{dayCode}_{timeCode}";
    }

    private string GetDayCode(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => "MON",
            DayOfWeek.Tuesday => "TUE",
            DayOfWeek.Wednesday => "WED",
            DayOfWeek.Thursday => "THU",
            DayOfWeek.Friday => "FRI",
            DayOfWeek.Saturday => "SAT",
            DayOfWeek.Sunday => "SUN",

            _ => throw new ArgumentOutOfRangeException(
                nameof(dayOfWeek),
                dayOfWeek,
                "Thứ trong tuần không hợp lệ."
            )
        };
    }

    private string RemoveVietnameseDiacritics(string value)
    {
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
