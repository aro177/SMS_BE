namespace Student_Management_System.Dtos.Classrooms;

public record ClassroomResponse(
    long Id,
    string Name,
    decimal TuitionFee,
    long? TeacherId,
    string? TeacherName,
    int StudentsCount);

public record CreateClassroomRequest(string Name, decimal TuitionFee, long? TeacherId);

public record UpdateClassroomRequest(string Name, decimal TuitionFee, long? TeacherId);
