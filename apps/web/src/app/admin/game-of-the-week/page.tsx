import { AdminGameOfTheWeekSection } from "@/components/admin/admin-game-of-the-week-section";
import type { GameOfTheWeekHistoryItemDto } from "@/generated/api-client";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function AdminGameOfTheWeekPage() {
  let currentPoll = null;
  let history: GameOfTheWeekHistoryItemDto[] = [];
  let loadError: string | null = null;
  let forbidden = false;

  try {
    const api = await getServerApiClient();
    try {
      currentPoll = await api.getAdminGameOfTheWeekCurrentPoll();
    } catch {
      currentPoll = null;
    }
    try {
      const historyResponse = await api.getGameOfTheWeekHistory(15, 0);
      history = historyResponse.items;
    } catch {
      history = [];
    }
  } catch (error) {
    const message = error instanceof Error ? error.message : "Failed to load game of the week";
    forbidden = message.includes("403");
    loadError = forbidden ? "Your account is not an administrator on the API." : message;
  }

  if (loadError) {
    return (
      <div className="mx-auto max-w-lg space-y-4 py-12">
        <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)]">
          {forbidden ? "Not authorized" : "Could not load"}
        </h1>
        <p className="text-muted-foreground">{loadError}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <p className="text-sm text-muted-foreground">
        Start voting periods, preview announcements, and review ballot results. Winners are imported automatically
        after polls close.
      </p>
      <AdminGameOfTheWeekSection currentPoll={currentPoll} history={history} />
    </div>
  );
}
