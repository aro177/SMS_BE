namespace Student_Management_System.Dtos.Classrooms;

public record ClassroomResponse(
    long Id,
    string Name,
    decimal TuitionFee,
    long? TeacherId,
    string? TeacherName,
    int StudentsCount,
    string? AgeGroup,
    string? Description,
    int Capacity);

public record CreateClassroomRequest(
    string Name,
    decimal TuitionFee,
    long? TeacherId,
    string? AgeGroup = null,
    string? Description = null,
    int Capacity = 20);

public record UpdateClassroomRequest(
    string Name,
    decimal TuitionFee,
    long? TeacherId,
    string? AgeGroup = null,
    string? Description = null,
    int Capacity = 20);
