"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { clearStoredToken, getStoredToken } from "@/lib/api/client";
import { useEffect, useState } from "react";

const nav = [
  { href: "/dashboard", label: "Dashboard" },
  { href: "/finance/fees", label: "Fee & Collections" },
  { href: "/students/STU-2026-0842", label: "Student 360" },
  { href: "/academics/timetable", label: "Timetable" },
];

export function AppShell({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const [ready, setReady] = useState(false);

  useEffect(() => {
    if (pathname === "/login") {
      setReady(true);
      return;
    }
    if (!getStoredToken()) router.replace("/login");
    else setReady(true);
  }, [pathname, router]);

  if (pathname === "/login") return <>{children}</>;
  if (!ready) return null;

  return (
    <div className="min-h-screen flex flex-col">
      <header className="bg-navy text-white px-6 py-3 flex items-center justify-between">
        <div>
          <p className="text-lg font-semibold">School PlayGround ERP</p>
          <p className="text-xs text-blue-200">Academic Year 2026–27 · Term 1 & 2</p>
        </div>
        <button
          type="button"
          className="text-xs border border-blue-300/40 rounded px-2 py-1 hover:bg-blue-800"
          onClick={() => {
            clearStoredToken();
            router.push("/login");
          }}
        >
          Sign out
        </button>
      </header>
      <div className="flex flex-1">
        <nav className="w-56 bg-white border-r border-slate-200 p-3 space-y-1">
          {nav.map((item) => {
            const active =
              item.href === "/dashboard"
                ? pathname === "/dashboard"
                : pathname.startsWith(item.href.split("/STU")[0]);
            return (
              <Link
                key={item.href}
                href={item.href}
                className={`block rounded-md px-3 py-2 text-sm ${
                  active
                    ? "bg-navy text-white"
                    : "text-slate-700 hover:bg-slate-100"
                }`}
              >
                {item.label}
              </Link>
            );
          })}
        </nav>
        <main className="flex-1 p-6 overflow-auto">{children}</main>
      </div>
    </div>
  );
}
