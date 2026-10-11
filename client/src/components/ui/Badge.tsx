type Variant = "success" | "warning" | "critical" | "neutral";

const styles: Record<Variant, string> = {
  success: "bg-emerald/15 text-emerald border-emerald/30",
  warning: "bg-amber/15 text-amber-dark border-amber/40",
  critical: "bg-crimson/10 text-crimson border-crimson/30",
  neutral: "bg-slate-100 text-slate-700 border-slate-200",
};

export function Badge({
  children,
  variant = "neutral",
}: {
  children: React.ReactNode;
  variant?: Variant;
}) {
  return (
    <span
      className={`inline-flex items-center rounded px-2 py-0.5 text-xs font-medium border ${styles[variant]}`}
    >
      {children}
    </span>
  );
}
