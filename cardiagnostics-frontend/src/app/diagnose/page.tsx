"use client";

import { useRef, useState } from "react";
import Link from "next/link";
import { submitProblemByPlate } from "@/lib/api/carApi";
import { Car, AlertTriangle, Wrench, Link as LinkIcon, DollarSign } from "lucide-react";

export default function DiagnosePage() {
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");

  const [plate, setPlate] = useState("");
  const [description, setDescription] = useState("");

  const [result, setResult] = useState<any>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  // 🔹 אינדיקציה ל-Cold Start
  const [showWarmupHint, setShowWarmupHint] = useState(false);

  // ✅ ref כדי להחזיר פוקוס לתיאור התקלה אחרי "אבחון נוסף"
  const descriptionRef = useRef<HTMLTextAreaElement | null>(null);

  async function onSubmit() {
    setLoading(true);
    setError("");
    setResult(null);
    setShowWarmupHint(false);

    // ⏱️ אחרי 20 שניות נציג הודעת warm-up
    const warmupTimer = setTimeout(() => {
      setShowWarmupHint(true);
    }, 20000);

    try {
      const response = await submitProblemByPlate({
        username,
        email,
        licensePlate: plate,
        problemDescription: description,
      });

      setResult(response);
    } catch (e: any) {
      setError(e.message || "שגיאה בשליחת הבקשה");
    } finally {
      clearTimeout(warmupTimer);
      setShowWarmupHint(false);
      setLoading(false);
    }
  }

  // ✅ "אבחון נוסף" – מאפס תוצאה + שגיאות + תיאור ומחזיר פוקוס
  function onNewDiagnosis() {
    setResult(null);
    setError("");
    setLoading(false);
    setShowWarmupHint(false);
    setDescription("");

    setTimeout(() => {
      descriptionRef.current?.focus();
    }, 0);
  }

  return (
    <main className="min-h-screen flex justify-center p-6">
      <div className="w-full max-w-xl space-y-4 rounded-3xl border border-white/10 bg-white/5 backdrop-blur p-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold">אבחון תקלה</h1>
          <Link href="/" className="text-sm underline opacity-80">
            חזרה
          </Link>
        </div>

        <div>
          <label className="block text-sm mb-1">שם משתמש</label>
          <input
            className="w-full rounded-xl border border-white/10 bg-white/5 text-white p-3 outline-none"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            placeholder="dor"
          />
        </div>

        <div>
          <label className="block text-sm mb-1">אימייל</label>
          <input
            type="email"
            className="w-full rounded-xl border border-white/10 bg-white/5 text-white p-3 outline-none"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="dor@example.com"
          />
        </div>

        <div>
          <label className="block text-sm mb-1">מספר רכב</label>
          <input
            className="w-full rounded-xl border border-white/10 bg-white/5 text-white p-3 outline-none"
            value={plate}
            onChange={(e) => setPlate(e.target.value)}
            placeholder="12-345-67"
          />
        </div>

        <div>
          <label className="block text-sm mb-1">תיאור תקלה</label>
          <textarea
            ref={descriptionRef}
            className="w-full rounded-xl border border-white/10 bg-white/5 text-white p-3 outline-none"
            rows={4}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="הרכב לא מניע, יש רעש קליקים..."
          />
        </div>

        <button
          onClick={onSubmit}
          disabled={!username || !email || !plate || !description || loading}
          className="w-full rounded-xl bg-white text-slate-900 p-3 font-semibold disabled:opacity-50 hover:opacity-90 transition"
        >
          {loading ? "שולח..." : "שלח תקלה"}
        </button>

        {/* ✅ כפתור "אבחון נוסף" */}
        {result && (
          <button
            type="button"
            onClick={onNewDiagnosis}
            className="w-full rounded-xl border border-white/15 bg-white/10 text-white p-3 font-semibold hover:bg-white/15 transition"
          >
            אבחון נוסף
          </button>
        )}

        {/* 🔹 Loader + הודעת warm-up */}
        {loading && (
          <div className="rounded-2xl border border-white/10 bg-white/5 backdrop-blur p-4 text-sm">
            <div className="font-semibold">המערכת מנתחת את התקלה…</div>
            {showWarmupHint && (
              <div className="mt-2 opacity-80">
                בפעם הראשונה זה יכול לקחת כמה דקות – המערכת מתחממת (Cold Start).
              </div>
            )}
          </div>
        )}

        {error && (
          <div className="rounded-2xl border border-red-300/30 bg-red-500/10 p-4">
            {error}
          </div>
        )}

        {result && <ResultCards result={result} />}
      </div>
    </main>
  );
}

/* ---------- Helpers ---------- */

function safeText(v: any): string {
  if (v == null) return "";
  if (typeof v === "string") return v;
  if (typeof v === "number" || typeof v === "boolean") return String(v);
  return "";
}

function extractNumericCost(costText: string): number | null {
  if (!costText) return null;

  const text = costText.replace(/\u00A0/g, " "); // רווחים מוזרים

  // ✅ תופס מחיר רק אם יש מטבע לידו (לפני או אחרי)
  // דוגמאות: "1,200 ₪" | "₪ 1200" | "1200 ש״ח" | "NIS 1200"
  const withCurrency =
    text.match(/(₪|ש"ח|ש״ח|NIS)\s*(\d{1,3}(?:,\d{3})*|\d+)/i) ||
    text.match(/(\d{1,3}(?:,\d{3})*|\d+)\s*(₪|ש"ח|ש״ח|NIS)/i);

  if (!withCurrency) return null;

  // המספר יכול להיות בקבוצה 2 או 1 לפי ההתאמה
  const numStr = (withCurrency[2] ?? withCurrency[1]).toString().replace(/,/g, "");
  const value = Number(numStr);

  return Number.isFinite(value) ? value : null;
}



function asArray(v: any): any[] {
  return Array.isArray(v) ? v : [];
}

// ✅ מחלק לפסקאות לפי \n ואם אין — לפי נקודה/שאלה/קריאה
function splitToParagraphs(text: string): string[] {
  if (!text) return [];

  const cleaned = text.replace(/\r/g, "").trim();

  const byNewlines = cleaned
    .split(/\n+/)
    .map((s) => s.trim())
    .filter(Boolean);

  if (byNewlines.length >= 2) return byNewlines;

  const bySentences = cleaned
    .split(/(?<=[.!?])\s+/)
    .map((s) => s.trim())
    .filter(Boolean);

  const paragraphs: string[] = [];
  for (let i = 0; i < bySentences.length; i += 2) {
    paragraphs.push(bySentences.slice(i, i + 2).join(" "));
  }

  return paragraphs.length ? paragraphs : [cleaned];
}

/* ---------- Severity Badge ---------- */

const severityMap: Record<string, { label: string; className: string; icon: string }> = {
  Low: {
    label: "קל",
    className: "bg-green-500/15 text-green-300 border border-green-500/30",
    icon: "✅",
  },
  Medium: {
    label: "בינוני",
    className: "bg-yellow-500/15 text-yellow-300 border border-yellow-500/30",
    icon: "⚠️",
  },
  High: {
    label: "חמור",
    className: "bg-red-500/15 text-red-300 border border-red-500/30",
    icon: "🛑",
  },
};

function normalizeSeverity(input: string) {
  const s = (input || "").trim();
  if (!s) return "";

  if (severityMap[s]) return s;

  const lower = s.toLowerCase();
  if (lower === "low") return "Low";
  if (lower === "medium") return "Medium";
  if (lower === "high") return "High";

  if (s.includes("קל")) return "Low";
  if (s.includes("בינוני")) return "Medium";
  if (s.includes("חמור")) return "High";

  return "";
}

function SeverityBadge({ severity }: { severity: string }) {
  const normalized = normalizeSeverity(severity);
  if (!normalized) return null;

  const meta = severityMap[normalized] || severityMap.Low;

  return (
    <span
      className={`inline-flex items-center gap-2 rounded-full px-3 py-1 text-xs font-semibold ${meta.className}`}
      title={`חומרה: ${meta.label}`}
    >
      <span aria-hidden>{meta.icon}</span>
      {meta.label}
    </span>
  );
}

function CostBadge({ costText }: { costText: string }) {
  const cost = extractNumericCost(costText);
 if (!cost) {
  return (
    <span className="inline-flex items-center gap-2 rounded-full px-3 py-1 text-xs font-semibold bg-white/10 text-white/70 border border-white/10">
      💰 אין הערכת מחיר
    </span>
  );
}


  let className = "";

  if (cost <= 500) {
    className = "bg-green-500/15 text-green-300 border border-green-500/30";
  } else if (cost <= 1000) {
    className = "bg-yellow-500/15 text-yellow-300 border border-yellow-500/30";
  } else if (cost <= 5000) {
    className = "bg-orange-500/15 text-orange-300 border border-orange-500/30";
  } else {
    className = "bg-red-500/15 text-red-300 border border-red-500/30";
  }

  // פורמט נעים למספר (1,200)
  const formatted = cost.toLocaleString("he-IL");

  return (
    <span
      className={`inline-flex items-center gap-2 rounded-full px-3 py-1 text-xs font-semibold ${className}`}
      title={`הערכת מחיר`}
    >
      💰 {formatted} ₪
    </span>
  );
}


/* ---------- Result UI ---------- */

function ResultCards({ result }: { result: any }) {
  const title =
    safeText(result?.problemName) ||
    safeText(result?.title) ||
    safeText(result?.summaryTitle) ||
    "תוצאות אבחון";

  const summary =
    safeText(result?.solution) ||
    safeText(result?.description) ||
    safeText(result?.summary) ||
    safeText(result?.answer) ||
    safeText(result?.message);

  const problemText =
    safeText(result?.problemDescription) || safeText(result?.problem) || "";

  const steps =
    asArray(result?.solutionSteps) ||
    asArray(result?.steps) ||
    asArray(result?.recommendations);

  // תומך גם במבנה של Links כאובייקט וגם כמערך
  const linksArray =
    asArray(result?.links) ||
    asArray(result?.relevantLinks) ||
    asArray(result?.manualLinks);

  const linksObject =
    result?.Links && typeof result.Links === "object" ? result.Links : null;

  const severity = safeText(result?.Severity || result?.severity);
  const estimatedCost = safeText(result?.EstimatedCost || result?.estimatedCost);

  // ✅ שדות מורחבים מפרטי הרכב (כמו בפוסטמן)
  const trimLevels = safeText(result?.trim_levels || result?.trimLevels);
  const lastTest = safeText(result?.last_test || result?.lastTest);
  const licenseValidUntil = safeText(result?.license_valid_until || result?.licenseValidUntil);
  const ownership = safeText(result?.ownership);

  const hasStructured =
    Boolean(summary) || steps.length > 0 || linksArray.length > 0 || linksObject;

  const hasCarDetails = Boolean(
    result?.licensePlate ||
      result?.manufacturer ||
      result?.model ||
      result?.year ||
      trimLevels ||
      ownership ||
      lastTest ||
      licenseValidUntil
  );

  return (
    <div className="space-y-4">
      {/* סיכום */}
      <div className="rounded-2xl border border-white/10 bg-white/5 backdrop-blur p-4">
       <div className="flex items-center justify-between gap-3">
  <div className="text-lg font-bold">{title}</div>

  <div className="flex items-center gap-2">
    {severity && <SeverityBadge severity={severity} />}
<CostBadge costText={estimatedCost || summary || ""} />
  </div>
</div>


        {/* ✅ פרטי רכב (מורחב) */}
        {hasCarDetails && (
          <div className="mt-3 rounded-xl border border-white/10 bg-white/5 p-3">
            <div className="flex items-center gap-2 font-semibold">
              <Car className="w-4 h-4" />
              פרטי רכב
            </div>

            <div className="mt-2 grid grid-cols-2 gap-2 text-sm text-white/80">
              {result?.licensePlate && (
                <div>
                  <span className="text-white/60">מספר רכב:</span>{" "}
                  {safeText(result.licensePlate)}
                </div>
              )}
              {result?.manufacturer && (
                <div>
                  <span className="text-white/60">יצרן:</span>{" "}
                  {safeText(result.manufacturer)}
                </div>
              )}
              {result?.model && (
                <div>
                  <span className="text-white/60">דגם:</span> {safeText(result.model)}
                </div>
              )}
              {result?.year && (
                <div>
                  <span className="text-white/60">שנה:</span> {safeText(result.year)}
                </div>
              )}

              {trimLevels && (
                <div>
                  <span className="text-white/60">רמת גימור:</span> {trimLevels}
                </div>
              )}
              {ownership && (
                <div>
                  <span className="text-white/60">בעלות:</span> {ownership}
                </div>
              )}
              {lastTest && (
                <div>
                  <span className="text-white/60">טסט אחרון:</span> {lastTest}
                </div>
              )}
              {licenseValidUntil && (
                <div>
                  <span className="text-white/60">רישיון עד:</span> {licenseValidUntil}
                </div>
              )}
            </div>
          </div>
        )}

        {problemText && (
          <p className="mt-3 text-white/70">
            <span className="font-semibold">מה תיארת:</span> {problemText}
          </p>
        )}

        {/* מחיר תיקון (אם הגיע כשדה נפרד) */}
        {estimatedCost && (
          <div className="mt-3 flex items-center gap-2 text-white/80">
            <DollarSign className="w-4 h-4" />
            <span className="font-semibold">עלות משוערת:</span>
            <span>{estimatedCost}</span>
          </div>
        )}

        {summary ? (
          <div className="mt-3 space-y-3 text-white/80 leading-relaxed">
            {splitToParagraphs(summary).map((p, idx) => (
              <p key={idx}>{p}</p>
            ))}
          </div>
        ) : (
          <p className="mt-2 text-white/60">
            קיבלנו תשובה מהשרת, אבל המבנה שלה לא סטנדרטי עדיין.
          </p>
        )}
      </div>

      {/* צעדים */}
      {steps.length > 0 && (
        <div className="rounded-2xl border border-white/10 bg-white/5 backdrop-blur p-4">
          <div className="flex items-center gap-2 font-semibold mb-2">
            <Wrench className="w-4 h-4" />
            צעדים מומלצים
          </div>

          <ol className="list-decimal pr-5 space-y-2 text-white/80">
            {steps.map((s: any, idx: number) => (
              <li key={idx}>{safeText(s) || JSON.stringify(s)}</li>
            ))}
          </ol>
        </div>
      )}

      {/* קישורים - תומך גם באובייקט וגם במערך */}
      {(linksArray.length > 0 || linksObject) && (
        <div className="rounded-2xl border border-white/10 bg-white/5 backdrop-blur p-4">
          <div className="flex items-center gap-2 font-semibold mb-2">
            <LinkIcon className="w-4 h-4" />
            קישורים מתוך ספר הרכב
          </div>

          <ul className="space-y-2 text-white/80">
            {/* אובייקט Links: { "Steering wheel": "https://..." } */}
            {linksObject &&
              Object.entries(linksObject).map(([label, url], idx) => (
                <li
                  key={`obj-${idx}`}
                  className="flex items-start justify-between gap-3"
                >
                  <span className="text-white/70">{String(label)}</span>
                  {url ? (
                    <a
                      href={String(url)}
                      target="_blank"
                      rel="noreferrer"
                      className="underline text-white hover:opacity-90 break-all"
                    >
                      לפתיחה
                    </a>
                  ) : (
                    <span className="text-white/50">—</span>
                  )}
                </li>
              ))}

            {/* מערך Links */}
            {linksArray.map((l: any, idx: number) => {
              const label =
                safeText(l?.label) || safeText(l?.title) || `קישור ${idx + 1}`;
              const url = safeText(l?.url) || safeText(l?.link) || safeText(l);

              return (
                <li
                  key={`arr-${idx}`}
                  className="flex items-start justify-between gap-3"
                >
                  <span className="text-white/70">{label}</span>
                  {url ? (
                    <a
                      href={url}
                      target="_blank"
                      rel="noreferrer"
                      className="underline text-white hover:opacity-90 break-all"
                    >
                      לפתיחה
                    </a>
                  ) : (
                    <span className="text-white/50">—</span>
                  )}
                </li>
              );
            })}
          </ul>
        </div>
      )}

      {/* fallback debug */}
      {!hasStructured && (
        <div className="rounded-2xl border border-white/10 bg-white/5 backdrop-blur p-4">
          <div className="flex items-center gap-2 font-semibold mb-2">
            <AlertTriangle className="w-4 h-4" />
            Raw (Debug)
          </div>

          <pre className="text-xs whitespace-pre-wrap text-white/70">
            {JSON.stringify(result, null, 2)}
          </pre>
        </div>
      )}
    </div>
  );
}
