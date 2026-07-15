namespace Student_Management_System.Dtos.ClassRegistrations;

public record ClassRegistrationRequest(
    long ClassId,
    string ChildName,
    DateOnly ChildDob,
    string ParentName,
    string ParentPhone,
    string? Note);

public record ClassRegistrationResponse(string Fullname, string Classroom, string Status);

public record ClassRegistrationItemResponse(
    long Id,
    long StudentId,
    long ClassroomId,
    string ChildName,
    string ParentName,
    string ParentPhone,
    string RequestedClass,
    DateOnly EnrollDate,
    string SubmittedAt,
    string Status);
