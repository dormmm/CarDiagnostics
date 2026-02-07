import Link from "next/link";

export default function StartPage() {
  return (
    <main className="min-h-screen flex items-center justify-center p-6 bg-gradient-to-br from-slate-900 via-indigo-900 to-slate-800">
      <div className="w-full max-w-md rounded-3xl bg-white/5 border border-white/10 backdrop-blur p-6 space-y-6 text-white text-center">
        <h1 className="text-2xl font-bold">איך תרצה להמשיך?</h1>

        <Link
          href="/diagnose"
          className="block w-full rounded-xl bg-white text-slate-900 p-3 font-semibold hover:opacity-90 transition"
        >
          המשך כאורח
        </Link>

        <Link
          href="/login"
          className="block w-full rounded-xl bg-indigo-500/20 border border-indigo-400/30 p-3 font-semibold hover:bg-indigo-500/30 transition"
        >
          התחברות
        </Link>

        <Link
          href="/register"
          className="block w-full rounded-xl bg-emerald-500/20 border border-emerald-400/30 p-3 font-semibold hover:bg-emerald-500/30 transition"
        >
          הרשמה
        </Link>

        <Link href="/" className="text-sm underline opacity-70">
          חזרה
        </Link>
      </div>
    </main>
  );
}
