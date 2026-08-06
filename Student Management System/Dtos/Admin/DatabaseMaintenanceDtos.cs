namespace Student_Management_System.Dtos.Admin;

public record ResetDatabaseRequest(string Confirmation);

public record ResetDatabaseResponse(int TruncatedTables);
