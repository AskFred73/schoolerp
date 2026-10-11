"use client";

import { useEffect, useMemo, useState } from "react";
import {
  createColumnHelper,
  flexRender,
  getCoreRowModel,
  useReactTable,
} from "@tanstack/react-table";
import { apiFetch } from "@/lib/api/client";
import { Card } from "@/components/ui/Card";
import { KpiTile } from "@/components/ui/KpiTile";
import { Badge } from "@/components/ui/Badge";
import { formatInr } from "@/lib/format/inr";

type FeesSummary = {
  annualAssessment: number;
  totalRealized: number;
  totalOverdue: number;
  counterCollectionsToday: number;
};

type InvoiceRow = {
  id: string;
  invoiceNumber: string;
  admissionNo: string;
  studentName: string;
  classSection: string;
  wing: string;
  totalAmount: number;
  amountPaid: number;
  paymentStatus: string;
  dueDate: string;
};

const columnHelper = createColumnHelper<InvoiceRow>();

export default function FeesPage() {
  const [summary, setSummary] = useState<FeesSummary | null>(null);
  const [invoices, setInvoices] = useState<InvoiceRow[]>([]);
  const [status, setStatus] = useState("");
  const [lookup, setLookup] = useState("STU-2026-0842");
  const [amount, setAmount] = useState("5000");
  const [channel, setChannel] = useState("UPI");
  const [waiveFine, setWaiveFine] = useState(false);
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    apiFetch<FeesSummary>("/api/v1/finance/fees/summary").then(setSummary);
  }, []);

  useEffect(() => {
    const q = status ? `?status=${status}` : "";
    apiFetch<InvoiceRow[]>(`/api/v1/finance/fees/invoices${q}`).then(setInvoices);
  }, [status]);

  const columns = useMemo(
    () => [
      columnHelper.accessor("invoiceNumber", { header: "Invoice" }),
      columnHelper.accessor("admissionNo", { header: "Admission No." }),
      columnHelper.accessor("studentName", { header: "Student" }),
      columnHelper.accessor("classSection", { header: "Class" }),
      columnHelper.accessor("paymentStatus", {
        header: "Status",
        cell: (info) => {
          const v = info.getValue();
          const variant =
            v === "PAID" ? "success" : v === "OVERDUE" ? "critical" : "warning";
          return <Badge variant={variant}>{v}</Badge>;
        },
      }),
      columnHelper.accessor("totalAmount", {
        header: "Total",
        cell: (info) => formatInr(info.getValue()),
      }),
      columnHelper.accessor("amountPaid", {
        header: "Paid",
        cell: (info) => formatInr(info.getValue()),
      }),
    ],
    [],
  );

  const table = useReactTable({ data: invoices, columns, getCoreRowModel: getCoreRowModel() });

  async function collect() {
    setMessage(null);
    const matches = invoices.filter((i) => i.admissionNo === lookup);
    const inv = matches.find((i) => i.paymentStatus !== "PAID") ?? matches[0];
    if (!inv) {
      setMessage("No invoice row for admission lookup — clear status filter or refresh.");
      return;
    }
    try {
      const receipt = await apiFetch<{ receiptNumber: string; thermalLine: string }>(
        "/api/v1/finance/pos/collect",
        {
          method: "POST",
          body: JSON.stringify({
            admissionNo: lookup,
            invoiceIds: [inv.id],
            channel,
            amount: Number(amount),
            waiveLateFine: waiveFine,
          }),
        },
      );
      setMessage(`Collected · ${receipt.receiptNumber} · ${receipt.thermalLine}`);
      apiFetch<InvoiceRow[]>(`/api/v1/finance/fees/invoices${status ? `?status=${status}` : ""}`).then(
        setInvoices,
      );
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Collection failed");
    }
  }

  return (
    <div className="space-y-6">
      <h1 className="text-xl font-semibold">Fee & Collections Center</h1>
      {summary && (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4">
          <KpiTile label="Annual Assessment" value={summary.annualAssessment} currency />
          <KpiTile label="Realized Collections" value={summary.totalRealized} currency tone="success" />
          <KpiTile label="Overdue Balance" value={summary.totalOverdue} currency tone="critical" />
          <KpiTile label="Counter Today" value={summary.counterCollectionsToday} currency />
        </div>
      )}

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-4">
        <Card title="Invoice Register" className="xl:col-span-2">
          <div className="mb-3 flex gap-2">
            <select
              className="border border-slate-300 rounded-md text-sm px-2 py-1"
              value={status}
              onChange={(e) => setStatus(e.target.value)}
            >
              <option value="">All statuses</option>
              <option value="PAID">Paid in Full</option>
              <option value="PARTIAL">Partial Dues</option>
              <option value="OVERDUE">Overdue Defaulters</option>
            </select>
          </div>
          <div className="overflow-auto border border-slate-200 rounded-md">
            <table className="w-full text-sm">
              <thead className="bg-[var(--table-header)]">
                {table.getHeaderGroups().map((hg) => (
                  <tr key={hg.id}>
                    {hg.headers.map((h) => (
                      <th key={h.id} className="p-2 text-left font-medium">
                        {flexRender(h.column.columnDef.header, h.getContext())}
                      </th>
                    ))}
                  </tr>
                ))}
              </thead>
              <tbody>
                {table.getRowModel().rows.map((row) => (
                  <tr key={row.id} className="border-t border-slate-100">
                    {row.getVisibleCells().map((cell) => (
                      <td key={cell.id} className="p-2">
                        {flexRender(cell.column.columnDef.cell, cell.getContext())}
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>

        <Card title="POS Quick-Collect">
          <div className="space-y-3 text-sm">
            <label className="block">
              Student lookup
              <input
                className="mt-1 w-full border border-slate-300 rounded-md px-2 py-1.5"
                value={lookup}
                onChange={(e) => setLookup(e.target.value)}
              />
            </label>
            <label className="block">
              Channel
              <select
                className="mt-1 w-full border border-slate-300 rounded-md px-2 py-1.5"
                value={channel}
                onChange={(e) => setChannel(e.target.value)}
              >
                {["UPI", "NetBanking", "ChequeDD", "CashPOS", "Card"].map((c) => (
                  <option key={c} value={c}>{c}</option>
                ))}
              </select>
            </label>
            <label className="block">
              Amount (₹)
              <input
                className="mt-1 w-full border border-slate-300 rounded-md px-2 py-1.5"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
              />
            </label>
            <label className="flex items-center gap-2">
              <input type="checkbox" checked={waiveFine} onChange={(e) => setWaiveFine(e.target.checked)} />
              Late fine waiver (audited)
            </label>
            <button
              type="button"
              onClick={collect}
              className="w-full bg-navy text-white rounded-md py-2 font-medium"
            >
              Collect & print slip
            </button>
            {message && <p className="text-xs text-slate-600 break-all">{message}</p>}
          </div>
        </Card>
      </div>
    </div>
  );
}
