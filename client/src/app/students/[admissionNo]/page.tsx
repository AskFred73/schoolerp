"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { apiFetch } from "@/lib/api/client";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { formatInr } from "@/lib/format/inr";

type Student360 = {
  header: {
    admissionNo: string;
    rollNo: number;
    fullName: string;
    classSection: string;
    cbseRegNo?: string;
    aadhaarLinked: boolean;
    aadhaarMasked?: string;
    house?: string;
    busRoute?: string;
  };
  guardians: {
    relation: string;
    fullName: string;
    occupation?: string;
    phone: string;
    smsEnabled: boolean;
    isPrimary: boolean;
    address?: string;
    addressVerified: boolean;
  }[];
  medicalNotes: { note: string; recordedAt: string }[];
  attendance: {
    totalDays: number;
    present: number;
    absent: number;
    leave: number;
    days: { date: string; status: string }[];
  };
  academics: {
    examName: string;
    gpaPercent: number;
    rank: number;
    classStrength: number;
    marksBreakdownJson: string;
    appraisalNote?: string;
  };
  finance: {
    outstandingBalance: number;
    invoices: {
      invoiceNumber: string;
      term: number;
      total: number;
      paid: number;
      status: string;
      dueDate: string;
    }[];
  };
};

export default function Student360Page() {
  const params = useParams();
  const admissionNo = params.admissionNo as string;
  const [data, setData] = useState<Student360 | null>(null);

  useEffect(() => {
    apiFetch<Student360>(`/api/v1/students/${admissionNo}/360`).then(setData);
  }, [admissionNo]);

  if (!data) return <p className="text-slate-500">Loading student profile…</p>;

  const marks = JSON.parse(data.academics.marksBreakdownJson) as Record<string, number>;

  return (
    <div className="space-y-6">
      <header className="bg-card border border-slate-200 rounded-lg p-4">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <h1 className="text-xl font-semibold text-navy">{data.header.fullName}</h1>
            <p className="text-sm text-slate-600">
              {data.header.admissionNo} · Roll {data.header.rollNo} · {data.header.classSection}
            </p>
            <p className="text-xs text-slate-500 mt-1">
              CBSE {data.header.cbseRegNo} · {data.header.house} · Bus {data.header.busRoute}
            </p>
          </div>
          <div className="text-right text-sm">
            {data.header.aadhaarLinked && (
              <Badge variant="success">Aadhaar {data.header.aadhaarMasked}</Badge>
            )}
          </div>
        </div>
      </header>

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-4">
        <Card title="Guardian & Medical Dossier">
          <div className="space-y-4 text-sm">
            {data.guardians.map((g) => (
              <div key={g.fullName + g.relation} className="border-b border-slate-100 pb-2">
                <p className="font-medium">{g.relation}: {g.fullName}</p>
                <p className="text-slate-600">{g.occupation ?? "—"} · {g.phone}</p>
                {g.address && (
                  <p className="text-xs text-slate-500">
                    {g.address} {g.addressVerified && "· Verified"}
                  </p>
                )}
              </div>
            ))}
            <ul className="text-slate-700">
              {data.medicalNotes.map((m) => (
                <li key={m.recordedAt}>{m.note}</li>
              ))}
            </ul>
          </div>
        </Card>

        <Card title="Academic Record">
          <p className="text-sm">
            {data.academics.examName}: GPA {data.academics.gpaPercent}% · Rank {data.academics.rank}/
            {data.academics.classStrength}
          </p>
          <ul className="mt-2 text-sm grid grid-cols-2 gap-1">
            {Object.entries(marks).map(([k, v]) => (
              <li key={k}>{k}: {v}%</li>
            ))}
          </ul>
          {data.academics.appraisalNote && (
            <p className="mt-3 text-sm text-slate-600 border-t border-slate-100 pt-2">
              {data.academics.appraisalNote}
            </p>
          )}
        </Card>
      </div>

      <Card title="Attendance (session days)">
        <p className="text-sm mb-3">
          Present {data.attendance.present} · Absent {data.attendance.absent} · Leave {data.attendance.leave} ·{" "}
          {data.attendance.totalDays} school days
        </p>
        <div className="grid grid-cols-[repeat(auto-fill,minmax(10px,1fr))] gap-1 max-h-40 overflow-auto">
          {data.attendance.days.map((d) => (
            <span
              key={d.date}
              title={`${d.date} — ${d.status}`}
              className={`h-3 w-3 rounded-sm ${
                d.status === "Present"
                  ? "bg-emerald"
                  : d.status === "Absent"
                    ? "bg-crimson"
                    : "bg-amber"
              }`}
            />
          ))}
        </div>
      </Card>

      <Card title="Student Financial Ledger">
        <p className="text-sm mb-2">
          Outstanding: <span className="font-semibold text-crimson">{formatInr(data.finance.outstandingBalance)}</span>
        </p>
        <table className="w-full text-sm">
          <thead className="bg-[var(--table-header)]">
            <tr>
              <th className="p-2 text-left">Invoice</th>
              <th className="p-2 text-left">Term</th>
              <th className="p-2 text-left">Total</th>
              <th className="p-2 text-left">Paid</th>
              <th className="p-2 text-left">Status</th>
            </tr>
          </thead>
          <tbody>
            {data.finance.invoices.slice(0, 8).map((inv) => (
              <tr key={inv.invoiceNumber} className="border-t border-slate-100">
                <td className="p-2 font-mono text-xs">{inv.invoiceNumber}</td>
                <td className="p-2">{inv.term}</td>
                <td className="p-2">{formatInr(inv.total)}</td>
                <td className="p-2">{formatInr(inv.paid)}</td>
                <td className="p-2">{inv.status}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </Card>
    </div>
  );
}
