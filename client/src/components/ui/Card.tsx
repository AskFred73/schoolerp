export function Card({
  title,
  children,
  className = "",
}: {
  title?: string;
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <section
      className={`bg-card border border-slate-200 rounded-lg shadow-sm ${className}`}
    >
      {title && (
        <header className="px-4 py-3 border-b border-slate-100 text-sm font-semibold text-slate-800">
          {title}
        </header>
      )}
      <div className="p-4">{children}</div>
    </section>
  );
}
