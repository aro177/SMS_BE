namespace Student_Management_System.Dtos.Students;

public record StudentResponse(
    long Id,
    string Fullname,
    DateOnly? Dob,
    decimal? Height,
    decimal? Weight,
    long? ParentId,
    string? ParentName,
    string? ParentPhone,
    string? CurrentClass);

public record ChildSearchResponse(
    string ChildName,
    string DateOfBirth,
    string ParentPhone,
    string CurrentClass,
    string AttendanceRate,
    string LatestNote);

public record CreateStudentRequest(
    string Fullname,
    DateOnly? Dob,
    decimal? Height,
    decimal? Weight,
    string ParentName,
    string ParentPhone);
