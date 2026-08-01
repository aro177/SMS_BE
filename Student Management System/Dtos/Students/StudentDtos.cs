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
    List<string> CurrentClass);

public record ChildSearchResponse(
    long StudentId,
    string ChildName,
    string DateOfBirth,
    decimal? Height,
    decimal? Weight,
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
