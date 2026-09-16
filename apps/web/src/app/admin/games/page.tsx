import Link from "next/link";
import { AdminGameTrackQueueSection } from "@/components/admin/admin-game-track-queue-section";
import { AdminGamesSection } from "@/components/admin/admin-games-section";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function AdminGamesPage() {
  let games;
  let queue;
  try {
    const api = await getServerApiClient();
    [games, queue] = await Promise.all([api.getAdminGames(), api.getAdminGameTrackQueue()]);
  } catch (error) {
    const message = error instanceof Error ? error.message : "Failed to load games";
    const forbidden = message.includes("403");
    return (
      <div className="mx-auto max-w-lg space-y-4 py-12">
        <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)]">
          {forbidden ? "Not authorized" : "Could not load games"}
        </h1>
        <p className="text-muted-foreground">
          {forbidden ? "Your account is not an administrator on the API." : message}
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-10">
      <section className="space-y-2 rounded border border-border p-5 text-sm text-muted-foreground">
        <p>
          New games are tracked when they win{" "}
          <Link href="/admin/game-of-the-week" className="text-foreground hover:text-[var(--accent-retro)]">
            game-of-the-week
          </Link>{" "}
          voting.
        </p>
      </section>

      <AdminGameTrackQueueSection items={queue} />

      <AdminGamesSection games={games} />
    </div>
  );
}
