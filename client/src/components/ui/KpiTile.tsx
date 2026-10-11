import { formatInr } from "@/lib/format/inr";

export function KpiTile({
  label,
  value,
  sub,
  tone = "default",
  currency,
}: {
  label: string;
  value: string | number;
  sub?: string;
  tone?: "default" | "success" | "warning" | "critical";
  currency?: boolean;
}) {
  const toneClass =
    tone === "success"
      ? "text-emerald"
      : tone === "warning"
        ? "text-amber-dark"
        : tone === "critical"
          ? "text-crimson"
          : "text-navy";

  const display =
    currency && typeof value === "number" ? formatInr(value) : value;

  return (
    <div className="bg-card border border-slate-200 rounded-lg p-4 min-h-[108px]">
      <p className="text-xs font-medium text-slate-500 uppercase tracking-wide">
        {label}
      </p>
      <p className={`mt-2 text-2xl font-semibold tabular-nums ${toneClass}`}>
        {display}
      </p>
      {sub && <p className="mt-1 text-xs text-slate-500">{sub}</p>}
    </div>
  );
}
