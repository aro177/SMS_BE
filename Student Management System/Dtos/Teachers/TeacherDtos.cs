namespace Student_Management_System.Dtos.Teachers;

public record TeacherResponse(long Id, string Fullname, string? Phone, int ClassesCount);

public record CreateTeacherRequest(string Fullname, string? Phone);

public record UpdateTeacherRequest(string Fullname, string? Phone);
