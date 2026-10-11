using SchoolERP.Domain.Entities;

namespace SchoolERP.Domain.Services;

public record TimetableConflict(
    string ConflictType,
    string Message,
    Guid? SlotId,
    int DayOfWeek,
    int PeriodNumber);

public static class TimetableConflictDetector
{
    public static IReadOnlyList<TimetableConflict> Detect(IEnumerable<TimetableSlot> allSlots, Guid? focusClassId = null)
    {
        var list = allSlots.ToList();
        var conflicts = new List<TimetableConflict>();

        var groups = list.GroupBy(s => (s.DayOfWeek, s.PeriodNumber));
        foreach (var group in groups)
        {
            var byRoom = group.GroupBy(s => s.RoomNumber).Where(g => g.Count() > 1);
            foreach (var roomGroup in byRoom)
            {
                foreach (var slot in roomGroup)
                {
                    if (focusClassId.HasValue && slot.ClassId != focusClassId) continue;
                    conflicts.Add(new TimetableConflict(
                        "RoomCollision",
                        $"Room {slot.RoomNumber} double-booked on day {slot.DayOfWeek} period {slot.PeriodNumber}.",
                        slot.Id,
                        slot.DayOfWeek,
                        slot.PeriodNumber));
                }
            }

            var teachers = group.Where(s => s.TeacherId.HasValue).GroupBy(s => s.TeacherId!.Value).Where(g => g.Count() > 1);
            foreach (var teacherGroup in teachers)
            {
                foreach (var slot in teacherGroup)
                {
                    if (focusClassId.HasValue && slot.ClassId != focusClassId) continue;
                    conflicts.Add(new TimetableConflict(
                        "TeacherClash",
                        $"Teacher assigned to multiple classes on day {slot.DayOfWeek} period {slot.PeriodNumber}.",
                        slot.Id,
                        slot.DayOfWeek,
                        slot.PeriodNumber));
                }
            }
        }

        return conflicts.DistinctBy(c => (c.ConflictType, c.SlotId)).ToList();
    }
}
