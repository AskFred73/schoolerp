"use client";

import { useEffect, useState } from "react";
import { apiFetch } from "@/lib/api/client";
import { DEMO_CLASS_XA_ID } from "@/lib/api/config";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";

type TimetableResponse = {
  classId: string;
  className: string;
  academicYear: string;
  slots: {
    id: string;
    dayOfWeek: number;
    periodNumber: number;
    subjectCode: string;
    teacherName?: string;
    substituteTeacherName?: string;
    roomNumber: string;
  }[];
  subjectQuotas: {
    subjectCode: string;
    scheduled: number;
    quota: number;
    isCompliant: boolean;
  }[];
  conflicts: {
    conflictType: string;
    message: string;
    slotId?: string;
    dayOfWeek: number;
    periodNumber: number;
  }[];
};

const days = ["Mon", "Tue", "Wed", "Thu", "Fri"];

export default function TimetablePage() {
  const [data, setData] = useState<TimetableResponse | null>(null);
  const [substituteSlotId, setSubstituteSlotId] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    apiFetch<TimetableResponse>(`/api/v1/academics/timetable/${DEMO_CLASS_XA_ID}`).then(setData);
  }, []);

  function cell(day: number, period: number) {
    return data?.slots.find((s) => s.dayOfWeek === day && s.periodNumber === period);
  }

  async function allocateSubstitute() {
    if (!substituteSlotId || !data) return;
    setMessage(null);
    try {
      await apiFetch("/api/v1/academics/substitutions/allocate", {
        method: "POST",
        body: JSON.stringify({
          timetableSlotId: substituteSlotId,
          substituteTeacherId: "77777777-7777-7777-7777-777777777777",
          reason: "Planned leave — UI allocation",
        }),
      });
      setMessage("Substitute allocated (SMS stub logged on API).");
      apiFetch<TimetableResponse>(`/api/v1/academics/timetable/${DEMO_CLASS_XA_ID}`).then(setData);
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Allocation failed");
    }
  }

  if (!data) return <p className="text-slate-500">Loading timetable…</p>;

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-xl font-semibold">
          Class Timetable — {data.className} ({data.academicYear})
        </h1>
        <div className="flex flex-wrap gap-2">
          {data.subjectQuotas.map((q) => (
            <Badge key={q.subjectCode} variant={q.isCompliant ? "success" : "warning"}>
              {q.subjectCode} {q.scheduled}/{q.quota}
            </Badge>
          ))}
        </div>
      </div>

      {data.conflicts.length > 0 && (
        <Card title="Conflict Telemetry">
          <ul className="text-sm space-y-1 text-crimson">
            {data.conflicts.map((c, i) => (
              <li key={`${c.conflictType}-${i}`}>{c.message}</li>
            ))}
          </ul>
        </Card>
      )}

      <Card title="Weekly Master Grid (8 periods)">
        <div className="overflow-auto">
          <table className="w-full text-xs border border-slate-200">
            <thead className="bg-[var(--table-header)]">
              <tr>
                <th className="p-2 border border-slate-200">Period</th>
                {days.map((d, idx) => (
                  <th key={d} className="p-2 border border-slate-200">{d}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {Array.from({ length: 8 }, (_, p) => p + 1).map((period) => (
                <tr key={period}>
                  <td className="p-2 border border-slate-100 font-medium">P{period}</td>
                  {days.map((dayLabel, dayIdx) => {
                    const slot = cell(dayIdx + 1, period);
                    const conflict = data.conflicts.some(
                      (c) => c.dayOfWeek === dayIdx + 1 && c.periodNumber === period,
                    );
                    return (
                      <td
                        key={`${dayLabel}-${period}`}
                        className={`p-2 border border-slate-100 align-top min-w-[120px] ${
                          conflict ? "bg-red-50" : ""
                        }`}
                      >
                        {slot ? (
                          <button
                            type="button"
                            className="text-left w-full"
                            onClick={() => setSubstituteSlotId(slot.id)}
                          >
                            <p className="font-semibold text-navy">{slot.subjectCode}</p>
                            <p>{slot.teacherName ?? "—"}</p>
                            <p className="text-slate-500">{slot.roomNumber}</p>
                            {slot.substituteTeacherName && (
                              <p className="text-emerald">Sub: {slot.substituteTeacherName}</p>
                            )}
                          </button>
                        ) : (
                          "—"
                        )}
                      </td>
                    );
                  })}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>

      {substituteSlotId && (
        <Card title="Substitute Allocation Drawer">
          <p className="text-sm mb-2">Slot selected. Assign Dr. K. Narayanan (demo).</p>
          <button
            type="button"
            className="bg-navy text-white text-sm px-3 py-2 rounded-md"
            onClick={allocateSubstitute}
          >
            Allocate substitute
          </button>
          {message && <p className="text-xs mt-2 text-slate-600">{message}</p>}
        </Card>
      )}
    </div>
  );
}
