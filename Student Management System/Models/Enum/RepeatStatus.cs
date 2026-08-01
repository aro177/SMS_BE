using NpgsqlTypes;

namespace Student_Management_System.Models.Enum;

public enum RepeatStatus
{
    [PgName("FIXED")]
    FIXED,
    [PgName("TEMPORARY")]
    TEMPORARY
}
