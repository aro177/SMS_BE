using NpgsqlTypes;

namespace Student_Management_System.Models.Enum
{
    public enum EnrollmentStatus
    {
        [PgName("PENDING")]
        PENDING,
        [PgName("ACTIVE")]
        ACTIVE,
        [PgName("SUSPENDED")]
        SUSPENDED,
        [PgName("DROPPED")]
        DROPPED
    }
}
