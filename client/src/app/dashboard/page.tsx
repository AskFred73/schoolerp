"use client";

import { useEffect, useState } from "react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { apiFetch } from "@/lib/api/client";
import { Card } from "@/components/ui/Card";
import { KpiTile } from "@/components/ui/KpiTile";
import { Badge } from "@/components/ui/Badge";
import { formatInr } from "@/lib/format/inr";

type DashboardSummary = {
  enrollment: { total: number; male: number; female: number; yoyChangePercent: number };
  attendance: { dailyPercent: number; cutoffTime: string };
  staff: { active: number; total: number; approvedLeaves: number; substitutionsAllocated: number };
  term1Revenue: { realized: number; target: number; realizedPercent: number; overdue: number };
  dailyInflowByChannel: { channel: string; amount: number }[];
  wingAttendance: { wing: string; percent: number }[];
  recentCollections: {
    receiptNumber: string;
    studentAdmissionNo: string;
    amount: number;
    channel: string;
    collectedAt: string;
  }[];
  approvalQueue: { id: string; type: string; tier: number; title: string; status: string }[];
};

export default function DashboardPage() {
  const [data, setData] = useState<DashboardSummary | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    apiFetch<DashboardSummary>("/api/v1/analytics/dashboard-summary")
      .then(setData)
      .catch((e) => setError(e.message));
  }, []);

  if (error) return <p className="text-crimson">{error}</p>;
  if (!data) return <p className="text-slate-500">Loading dashboard…</p>;

  return (
    <div className="space-y-6">
      <h1 className="text-xl font-semibold text-slate-900">Executive Admin Dashboard</h1>
      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4">
        <KpiTile
          label="Total Enrollment"
          value={data.enrollment.total.toLocaleString("en-IN")}
          sub={`M ${data.enrollment.male.toLocaleString("en-IN")} · F ${data.enrollment.female.toLocaleString("en-IN")} · YoY +${data.enrollment.yoyChangePercent}%`}
        />
        <KpiTile
          label="Daily Biometric Attendance"
          value={`${data.attendance.dailyPercent}%`}
          sub={`Cutoff ${data.attendance.cutoffTime}`}
          tone="success"
        />
        <KpiTile
          label="Teaching Staff Active"
          value={`${data.staff.active} / ${data.staff.total}`}
          sub={`${data.staff.approvedLeaves} leaves · ${data.staff.substitutionsAllocated} substitutions`}
        />
        <KpiTile
          label="Term-1 Revenue Realization"
          value={formatInr(data.term1Revenue.realized)}
          sub={`${data.term1Revenue.realizedPercent}% · Overdue ${formatInr(data.term1Revenue.overdue)}`}
          tone="warning"
        />
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-4">
        <Card title="Daily Inflow vs Channels" className="xl:col-span-2">
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={data.dailyInflowByChannel}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} />
                <XAxis dataKey="channel" tick={{ fontSize: 12 }} />
                <YAxis tickFormatter={(v) => `₹${(v / 100000).toFixed(0)}L`} />
                <Tooltip formatter={(v) => formatInr(Number(v ?? 0))} />
                <Bar dataKey="amount" fill="#1e3a8a" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </Card>
        <Card title="Wing Attendance">
          <ul className="space-y-3">
            {data.wingAttendance.map((w) => (
              <li key={w.wing} className="flex justify-between text-sm">
                <span>{w.wing}</span>
                <span className="font-medium text-emerald">{w.percent}%</span>
              </li>
            ))}
          </ul>
        </Card>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-4">
        <Card title="Collections Ticker (last 5)">
          <table className="w-full text-sm">
            <thead className="bg-[var(--table-header)] text-left">
              <tr>
                <th className="p-2">Receipt</th>
                <th className="p-2">Student</th>
                <th className="p-2">Amount</th>
                <th className="p-2">Channel</th>
              </tr>
            </thead>
            <tbody>
              {data.recentCollections.map((c) => (
                <tr key={c.receiptNumber} className="border-t border-slate-100">
                  <td className="p-2 font-mono text-xs">{c.receiptNumber}</td>
                  <td className="p-2">{c.studentAdmissionNo}</td>
                  <td className="p-2 tabular-nums">{formatInr(c.amount)}</td>
                  <td className="p-2">{c.channel}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </Card>
        <Card title="Operational Approval Queue">
          <ul className="space-y-3">
            {data.approvalQueue.map((a) => (
              <li key={a.id} className="flex items-start justify-between gap-2 border-b border-slate-100 pb-2">
                <div>
                  <p className="text-sm font-medium">{a.title}</p>
                  <p className="text-xs text-slate-500">Tier {a.tier} · {a.type}</p>
                </div>
                <Badge variant="warning">{a.status}</Badge>
              </li>
            ))}
          </ul>
        </Card>
      </div>
    </div>
  );
}
