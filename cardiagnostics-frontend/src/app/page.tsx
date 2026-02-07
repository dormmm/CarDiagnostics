import Link from "next/link";
import Image from "next/image";

export default function LandingPage() {
  return (
    <main className="min-h-screen flex items-center justify-center p-6 bg-gradient-to-br from-slate-900 via-indigo-900 to-slate-800">
      <div className="w-full max-w-4xl grid gap-8 md:grid-cols-2 items-center">
        
        {/* טקסט וכפתורים */}
        <div className="space-y-5 text-white">
          <h1 className="text-4xl font-extrabold">CarDiagnostics</h1>

          <p className="text-white/80 leading-relaxed">
            אבחון תקלות חכם לרכב.  
            כתוב מה הבעיה, הוסף מספר רכב וקבל הסבר ברור והכוונה מעשית.
          </p>

          <Link
  href="/start"
  className="inline-block bg-white text-slate-900 rounded-xl px-6 py-3 font-semibold hover:opacity-90 transition"
>
  להתחיל אבחון
</Link>
        </div>

        {/* תמונת רכב */}
        <div className="rounded-3xl overflow-hidden shadow-2xl border border-white/10">
          <Image
            src="/carOIP.webp"
            alt="Car diagnostics"
            width={1200}
            height={800}
            className="w-full h-[300px] object-cover"
            priority
          />
        </div>

      </div>
    </main>
  );
}
