using NpgsqlTypes;

namespace Student_Management_System.Models.Enum
{
    public enum AttendanceStatus
    {
        [PgName("PRESENT")]
        PRESENT,
        [PgName("ABSENT")]
        ABSENT,
        [PgName("LATE")]
        LATE,
        [PgName("EXCUSED")]
        EXCUSED
    }
}
