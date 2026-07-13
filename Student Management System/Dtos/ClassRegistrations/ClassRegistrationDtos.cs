namespace Student_Management_System.Dtos.ClassRegistrations;

public record ClassRegistrationRequest(
    long ClassId,
    string ChildName,
    DateOnly ChildDob,
    string ParentName,
    string ParentPhone,
    string? Note);

public record ClassRegistrationResponse(string Fullname, string Classroom, string Status);
