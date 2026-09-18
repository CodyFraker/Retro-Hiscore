import type { Metadata } from "next";
import { Space_Grotesk, IBM_Plex_Mono } from "next/font/google";
import { Toaster } from "sonner";
import { AppSessionProvider } from "@/components/session-provider";
import { SiteHeader } from "@/components/site-header";
import { ThemeInitScript } from "@/components/theme/theme-init-script";
import { RouteViewReporter } from "@/components/route-view-reporter";
import { ServerPageViewReporter } from "@/components/server-page-view-reporter";
import { ThemeProvider } from "@/components/theme/theme-provider";
import { getLayoutThemeState } from "@/lib/theme-layout";
import "./globals.css";

const display = Space_Grotesk({
  variable: "--font-display",
  subsets: ["latin"],
});

const body = Space_Grotesk({
  variable: "--font-body",
  subsets: ["latin"],
  weight: ["400", "500", "600", "700"],
});

const mono = IBM_Plex_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
  weight: ["400", "500"],
});

export const metadata: Metadata = {
  title: "Retro Hiscore",
  description: "Friend leaderboard tracker for RetroAchievements",
};

export default async function RootLayout({ children }: LayoutProps<"/">) {
  const { initialTheme, syncToAccount, serverTheme } = await getLayoutThemeState();

  return (
    <html
      lang="en"
      className={`${display.variable} ${body.variable} ${mono.variable} h-full antialiased`}
      data-theme={serverTheme ?? undefined}
      suppressHydrationWarning
    >
      <head>
        <ThemeInitScript enabled={serverTheme === null} />
      </head>
      <body className="min-h-full flex flex-col">
        <AppSessionProvider>
          <ThemeProvider initialTheme={initialTheme} syncToAccount={syncToAccount}>
            <ServerPageViewReporter />
            <RouteViewReporter />
            <SiteHeader />
            <main className="mx-auto min-w-0 w-full max-w-6xl flex-1 px-4 py-6 md:py-8">
              {children}
            </main>
            <Toaster richColors closeButton position="top-center" />
          </ThemeProvider>
        </AppSessionProvider>
      </body>
    </html>
  );
}
