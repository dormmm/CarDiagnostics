import "./globals.css";
import Header from "@/components/Header";

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="he">
      <body className="min-h-screen bg-gradient-to-br from-slate-900 via-indigo-900 to-slate-800 text-white">
        <Header />
        <main>{children}</main>
      </body>
    </html>
  );
}
