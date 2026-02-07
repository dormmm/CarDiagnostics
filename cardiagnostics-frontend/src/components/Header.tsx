"use client";

import Link from "next/link";
import Image from "next/image";
import { usePathname } from "next/navigation";

export default function Header() {
  const pathname = usePathname();

  const linkClass = (path: string) =>
    pathname === path
      ? "rounded-lg bg-white/15 px-3 py-1.5 font-semibold"
      : "opacity-80 hover:opacity-100 transition";

  return (
    <header className="sticky top-0 z-50 border-b border-white/10 bg-slate-950/70 backdrop-blur">
      <div className="mx-auto max-w-7xl px-6 py-4 flex items-center justify-between">

        {/* Logo */}
        <Link href="/" className="flex items-center gap-3">
          <Image
            src="/logo.jpg"
            alt="CarDiagnostics logo"
            width={36}
            height={36}
            priority
          />
          <span className="text-lg font-extrabold tracking-tight">
            CarDiagnostics
          </span>
        </Link>

        {/* Navigation */}
        <nav className="flex items-center gap-4 text-sm">
          <Link href="/" className={linkClass("/")}>
            בית
          </Link>

          <Link href="/diagnose" className={linkClass("/diagnose")}>
            אבחון
          </Link>

          <Link
            href="/login"
            className={
              pathname === "/login"
                ? "rounded-lg bg-white text-slate-900 px-3 py-1.5 font-semibold"
                : "rounded-lg border border-white/20 px-3 py-1.5 opacity-90 hover:bg-white/10 transition"
            }
          >
            התחברות
          </Link>
        </nav>
      </div>
    </header>
  );
}
