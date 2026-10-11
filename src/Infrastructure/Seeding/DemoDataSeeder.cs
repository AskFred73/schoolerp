using Microsoft.EntityFrameworkCore;
using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;
using SchoolERP.Infrastructure.Data;

namespace SchoolERP.Infrastructure.Seeding;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(SchoolErpDbContext db, CancellationToken ct = default)
    {
        if (await db.Tenants.AnyAsync(ct))
            return;

        var tenant = new Tenant
        {
            Id = DemoIds.TenantId,
            Name = "School PlayGround Demo",
            Code = "SPG-DEMO"
        };
        db.Tenants.Add(tenant);

        db.AcademicYears.Add(new AcademicYear
        {
            Id = Guid.NewGuid(),
            TenantId = DemoIds.TenantId,
            Label = "2026-27",
            IsCurrent = true
        });

        db.AppUsers.AddRange(
            new AppUser
            {
                Id = DemoIds.PrincipalUserId,
                TenantId = DemoIds.TenantId,
                Email = "principal@school.local",
                DisplayName = "Principal Admin",
                Role = "Principal",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo@123")
            },
            new AppUser
            {
                Id = DemoIds.AccountantUserId,
                TenantId = DemoIds.TenantId,
                Email = "accountant@school.local",
                DisplayName = "Fee Counter Clerk",
                Role = "Accountant",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo@123")
            },
            new AppUser
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                TenantId = DemoIds.TenantId,
                Email = "academic@school.local",
                DisplayName = "Academic Coordinator",
                Role = "AcademicCoordinator",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo@123")
            });

        var classXA = new SchoolClass
        {
            Id = DemoIds.ClassXAId,
            TenantId = DemoIds.TenantId,
            Name = "Class X",
            GradeLevel = 10,
            Wing = AcademicWing.Secondary
        };
        db.Classes.Add(classXA);

        db.Houses.Add(new House { Id = DemoIds.HouseTagoreId, TenantId = DemoIds.TenantId, Name = "Tagore House" });
        db.TransportRoutes.Add(new TransportRoute
        {
            Id = DemoIds.Route14Id,
            TenantId = DemoIds.TenantId,
            RouteNumber = 14,
            Description = "Sector 14 — City Centre Loop"
        });

        var staff = new List<Staff>();
        for (var i = 1; i <= 170; i++)
        {
            var id = Guid.NewGuid();
            if (i == 42)
                id = DemoIds.TeacherNarayananId;
            staff.Add(new Staff
            {
                Id = id,
                TenantId = DemoIds.TenantId,
                EmployeeCode = $"EMP-{i:D4}",
                FirstName = i == 42 ? "K." : $"Teacher{i}",
                LastName = i == 42 ? "Narayanan" : "Staff",
                PrimarySubjectCode = i % 3 == 0 ? "MTH-101" : i % 3 == 1 ? "PHY-102" : "ENG-103",
                IsActive = i <= 164
            });
        }
        db.Staff.AddRange(staff);

        db.Subjects.AddRange(
            new Subject { Id = Guid.NewGuid(), TenantId = DemoIds.TenantId, Code = "MTH-101", Name = "Mathematics", WeeklyPeriodQuota = 6 },
            new Subject { Id = Guid.NewGuid(), TenantId = DemoIds.TenantId, Code = "SCI-201", Name = "Science", WeeklyPeriodQuota = 8 },
            new Subject { Id = Guid.NewGuid(), TenantId = DemoIds.TenantId, Code = "CCS-301", Name = "Co-Curricular & Sports", WeeklyPeriodQuota = 8 });

        var hero = new Student
        {
            Id = DemoIds.Student0842Id,
            TenantId = DemoIds.TenantId,
            AdmissionNo = "STU-2026-0842",
            RollNo = 12,
            FirstName = "Aarav",
            LastName = "Sharma",
            ClassId = DemoIds.ClassXAId,
            Section = "A",
            HouseId = DemoIds.HouseTagoreId,
            CbseRegNo = "CBSE-2026-78421",
            AadhaarLinked = true,
            AadhaarLastFour = "4812",
            BusRouteId = DemoIds.Route14Id,
            Status = StudentStatus.ACTIVE
        };
        db.Students.Add(hero);

        for (var i = 1; i <= 24; i++)
        {
            if (i == 12) continue;
            db.Students.Add(new Student
            {
                Id = Guid.NewGuid(),
                TenantId = DemoIds.TenantId,
                AdmissionNo = $"STU-2026-{(800 + i):D4}",
                RollNo = i,
                FirstName = $"Student{i}",
                LastName = "Demo",
                ClassId = DemoIds.ClassXAId,
                Section = "A",
                Status = StudentStatus.ACTIVE
            });
        }

        db.Guardians.AddRange(
            new Guardian
            {
                Id = Guid.NewGuid(),
                TenantId = DemoIds.TenantId,
                StudentId = DemoIds.Student0842Id,
                Relation = GuardianRelation.Father,
                FullName = "Rajesh Sharma",
                Occupation = "Chartered Accountant",
                Phone = "+91-98765-43210",
                SmsEnabled = true,
                IsPrimary = true,
                AddressLine = "42, Lake View Colony, Bengaluru — 560001",
                AddressVerified = true
            },
            new Guardian
            {
                Id = Guid.NewGuid(),
                TenantId = DemoIds.TenantId,
                StudentId = DemoIds.Student0842Id,
                Relation = GuardianRelation.Mother,
                FullName = "Priya Sharma",
                Phone = "+91-98765-43211",
                SmsEnabled = true,
                IsPrimary = false
            });

        db.StudentMedicalNotes.Add(new StudentMedicalNote
        {
            Id = Guid.NewGuid(),
            TenantId = DemoIds.TenantId,
            StudentId = DemoIds.Student0842Id,
            Note = "Asthma — inhaler locker #7 assigned in infirmary."
        });

        var start = new DateOnly(2026, 4, 1);
        var rnd = new Random(42);
        for (var d = 0; d < 158; d++)
        {
            var date = start.AddDays(d);
            if (date.DayOfWeek is DayOfWeek.Sunday) continue;
            var status = rnd.NextDouble() < 0.92 ? AttendanceStatus.Present :
                rnd.NextDouble() < 0.5 ? AttendanceStatus.Absent : AttendanceStatus.Leave;
            db.AttendanceDays.Add(new AttendanceDay
            {
                Id = Guid.NewGuid(),
                TenantId = DemoIds.TenantId,
                StudentId = DemoIds.Student0842Id,
                Date = date,
                Status = status,
                CheckInTime = status == AttendanceStatus.Present ? new TimeOnly(8, rnd.Next(0, 25)) : null
            });
        }

        db.ExamResults.Add(new ExamResult
        {
            Id = Guid.NewGuid(),
            TenantId = DemoIds.TenantId,
            StudentId = DemoIds.Student0842Id,
            ExamName = "Mid-Term",
            GpaPercent = 92.2m,
            Rank = 3,
            ClassStrength = 42,
            MarksBreakdownJson = """{"Mathematics":94,"Science":91,"English":90,"Social":93}"""
        });

        db.AppraisalNotes.Add(new AppraisalNote
        {
            Id = Guid.NewGuid(),
            TenantId = DemoIds.TenantId,
            StudentId = DemoIds.Student0842Id,
            AuthorName = "Ms. Ananya Iyer (Class Teacher)",
            Note = "Consistent performer; leads science club activities with initiative."
        });

        var invoice = new FeeInvoice
        {
            Id = DemoIds.Invoice4412Id,
            TenantId = DemoIds.TenantId,
            InvoiceNumber = "INV-2026-4412",
            StudentId = DemoIds.Student0842Id,
            AcademicYear = "2026-27",
            Term = 1,
            TuitionFee = 45000m,
            LabSmartboardFee = 8500m,
            TransportFee = 12000m,
            LateFine = 500m,
            TotalAmount = 66000m,
            AmountPaid = 40000m,
            PaymentStatus = PaymentStatus.PARTIAL,
            DueDate = new DateOnly(2026, 8, 15)
        };
        db.FeeInvoices.Add(invoice);

        for (var i = 0; i < 15; i++)
        {
            db.FeeInvoices.Add(new FeeInvoice
            {
                Id = Guid.NewGuid(),
                TenantId = DemoIds.TenantId,
                InvoiceNumber = $"INV-2026-{4400 + i}",
                StudentId = DemoIds.Student0842Id,
                AcademicYear = "2026-27",
                Term = 1,
                TuitionFee = 40000 + i * 100,
                TotalAmount = 42000 + i * 100,
                AmountPaid = i % 3 == 0 ? 42000 + i * 100 : i % 3 == 1 ? 20000m : 0m,
                PaymentStatus = i % 3 == 0 ? PaymentStatus.PAID : i % 3 == 1 ? PaymentStatus.PARTIAL : PaymentStatus.OVERDUE,
                DueDate = new DateOnly(2026, 7, 1).AddDays(i * 5)
            });
        }

        var channels = new[] { PaymentChannel.UPI, PaymentChannel.NetBanking, PaymentChannel.ChequeDD, PaymentChannel.CashPOS, PaymentChannel.Card };
        for (var i = 0; i < 68; i++)
        {
            db.FeePayments.Add(new FeePayment
            {
                Id = Guid.NewGuid(),
                TenantId = DemoIds.TenantId,
                FeeInvoiceId = DemoIds.Invoice4412Id,
                StudentId = DemoIds.Student0842Id,
                Channel = channels[i % channels.Length],
                Amount = 5000 + (i * 137) % 15000,
                ReceiptNumber = $"RCP-2026-{1000 + i}",
                CollectedAt = DateTime.UtcNow.AddHours(-i)
            });
        }

        SeedTimetable(db, staff);

        db.StaffLeaves.AddRange(
            new StaffLeave { Id = Guid.NewGuid(), TenantId = DemoIds.TenantId, StaffId = staff[0].Id, StartDate = DateOnly.FromDateTime(DateTime.UtcNow), EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), IsApproved = true, Reason = "Medical" },
            new StaffLeave { Id = Guid.NewGuid(), TenantId = DemoIds.TenantId, StaffId = staff[1].Id, StartDate = DateOnly.FromDateTime(DateTime.UtcNow), EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), IsApproved = true, Reason = "Family" });

        db.ApprovalRequests.AddRange(
            new ApprovalRequest { Id = Guid.NewGuid(), TenantId = DemoIds.TenantId, Type = ApprovalType.TC, Tier = 1, Title = "TC clearance — STU-2026-0312", Description = "Awaiting library & fee nod.", Status = ApprovalStatus.Pending },
            new ApprovalRequest { Id = Guid.NewGuid(), TenantId = DemoIds.TenantId, Type = ApprovalType.MedicalSubstitute, Tier = 2, Title = "Biology period 4 — Mon", Description = "Dr. Mehta on medical leave.", Status = ApprovalStatus.Pending },
            new ApprovalRequest { Id = Guid.NewGuid(), TenantId = DemoIds.TenantId, Type = ApprovalType.EWSWaiver, Tier = 3, Title = "EWS waiver — Term 1", Description = "₹12,500 partial waiver request.", Status = ApprovalStatus.Pending },
            new ApprovalRequest { Id = Guid.NewGuid(), TenantId = DemoIds.TenantId, Type = ApprovalType.FleetDelay, Tier = 4, Title = "Route #14 delay alert", Description = "25 min delay — monsoon traffic.", Status = ApprovalStatus.Pending });

        db.DashboardSnapshots.Add(new DashboardSnapshot
        {
            Id = Guid.NewGuid(),
            TenantId = DemoIds.TenantId,
            TotalEnrollment = 3420,
            MaleEnrollment = 1785,
            FemaleEnrollment = 1635,
            EnrollmentYoyChangePercent = 4.2m,
            DailyAttendancePercent = 94.8m,
            StaffActive = 164,
            StaffTotal = 170,
            ApprovedLeavesToday = 4,
            SubstitutionsAllocated = 2,
            Term1RevenueTarget = 208800000m,
            Term1RevenueRealized = 184500000m,
            Term1Overdue = 2430000m,
            AnnualAssessment = 495900000m,
            TotalRealized = 392450000m,
            TotalOverdue = 48200000m,
            CounterCollectionsToday = 842500m,
            DailyTransactionCount = 68
        });

        await db.SaveChangesAsync(ct);
    }

    private static void SeedTimetable(SchoolErpDbContext db, List<Staff> staff)
    {
        var teacher = DemoIds.TeacherNarayananId;
        var otherTeacher = staff.First(s => s.Id != teacher).Id;
        var classXBId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        db.Classes.Add(new SchoolClass
        {
            Id = classXBId,
            TenantId = DemoIds.TenantId,
            Name = "Class XI",
            GradeLevel = 11,
            Wing = AcademicWing.SeniorSecondary
        });
        var subjectRotation = new[] { "MTH-101", "ENG-103", "SCI-201", "MTH-101", "CCS-301", "SCI-201", "MTH-101", "CCS-301" };
        var rooms = new[] { "X-A-01", "Bio Lab", "Chem Lab", "X-A-01", "Sports Arena", "Phy Lab", "X-A-01", "Bio Lab" };

        for (var day = 1; day <= 5; day++)
        {
            for (var period = 1; period <= 8; period++)
            {
                db.TimetableSlots.Add(new TimetableSlot
                {
                    Id = Guid.NewGuid(),
                    TenantId = DemoIds.TenantId,
                    ClassId = DemoIds.ClassXAId,
                    DayOfWeek = day,
                    PeriodNumber = period,
                    SubjectCode = subjectRotation[period - 1],
                    TeacherId = period == 2 && day == 1 ? teacher : otherTeacher,
                    RoomNumber = rooms[period - 1],
                    AcademicYear = "2026-27"
                });
            }
        }

        // Intentional room collision for conflict telemetry (Bio Lab period 2 Monday across another class slot)
        db.TimetableSlots.Add(new TimetableSlot
        {
            Id = Guid.NewGuid(),
            TenantId = DemoIds.TenantId,
            ClassId = classXBId,
            DayOfWeek = 1,
            PeriodNumber = 2,
            SubjectCode = "SCI-201",
            TeacherId = teacher,
            RoomNumber = "Bio Lab",
            AcademicYear = "2026-27"
        });
    }
}
