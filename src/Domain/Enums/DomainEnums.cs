namespace SchoolERP.Domain.Enums;

public enum PaymentChannel
{
    UPI,
    NetBanking,
    ChequeDD,
    CashPOS,
    Card
}

public enum PaymentStatus
{
    PAID,
    PARTIAL,
    OVERDUE
}

public enum StudentStatus
{
    ACTIVE,
    TRANSFERRED,
    ALUMNI
}

public enum AttendanceStatus
{
    Present,
    Absent,
    Leave,
    Holiday
}

public enum ApprovalType
{
    TC,
    MedicalSubstitute,
    EWSWaiver,
    FleetDelay
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}

public enum AcademicWing
{
    PrePrimary,
    Middle,
    Secondary,
    SeniorSecondary
}

public enum GuardianRelation
{
    Father,
    Mother,
    Guardian
}
