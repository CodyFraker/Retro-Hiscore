import { AlertCircle } from "lucide-react";
import { DashboardHero } from "@/components/dashboard/dashboard-hero";
import { DashboardLayout } from "@/components/dashboard/dashboard-layout";
import { DashboardSidebar } from "@/components/dashboard/dashboard-sidebar";
import { TrackedGamesSection } from "@/components/dashboard/tracked-games-section";
import type { DashboardResponse } from "@/generated/api-client";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function HomePage() {
  let dashboard: DashboardResponse | null = null;
  let error: string | null = null;

  try {
    const api = await getServerApiClient();
    dashboard = await api.getDashboard();
  } catch (err) {
    error = err instanceof Error ? err.message : "Failed to load dashboard";
  }

  const games = dashboard?.games ?? [];

  return (
    <div className="space-y-8">
      <DashboardHero />

      {error && (
        <p className="flex items-center gap-2 rounded border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm">
          <AlertCircle className="size-4 shrink-0" />
          {error}
        </p>
      )}

      {!error && (
        <DashboardLayout
          primary={
            <TrackedGamesSection games={games} />
          }
          sidebar={
            dashboard ? (
              <DashboardSidebar
                championship={dashboard.championship}
                recentGroupGames={dashboard.recentGroupGames}
              />
            ) : undefined
          }
        />
      )}
    </div>
  );
}
