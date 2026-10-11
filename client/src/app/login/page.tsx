"use client";

import { login } from "@/lib/api/client";
import { useRouter } from "next/navigation";
import { useState } from "react";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("accountant@school.local");
  const [password, setPassword] = useState("Demo@123");
  const [error, setError] = useState<string | null>(null);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      await login(email, password);
      router.push("/dashboard");
    } catch {
      setError("Invalid credentials");
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-canvas p-6">
      <form
        onSubmit={onSubmit}
        className="w-full max-w-md bg-card border border-slate-200 rounded-lg p-6 space-y-4"
      >
        <div>
          <h1 className="text-xl font-semibold text-navy">School PlayGround ERP</h1>
          <p className="text-sm text-slate-500">Sign in to the admin portal</p>
        </div>
        <label className="block text-sm">
          Email
          <input
            className="mt-1 w-full border border-slate-300 rounded-md px-3 py-2"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
        </label>
        <label className="block text-sm">
          Password
          <input
            type="password"
            className="mt-1 w-full border border-slate-300 rounded-md px-3 py-2"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
        </label>
        {error && <p className="text-sm text-crimson">{error}</p>}
        <button
          type="submit"
          className="w-full bg-navy text-white rounded-md py-2 text-sm font-medium"
        >
          Sign in
        </button>
        <p className="text-xs text-slate-500">
          Demo: principal@school.local · accountant@school.local · academic@school.local (password Demo@123)
        </p>
      </form>
    </div>
  );
}
