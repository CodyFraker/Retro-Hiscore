import { getServerSession } from "next-auth";
import { AddGameForm } from "@/components/add-game-form";
import { AdminGameTrackQueueSection } from "@/components/admin/admin-game-track-queue-section";
import { AdminGamesSection } from "@/components/admin/admin-games-section";
import { authOptions } from "@/lib/auth-options";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function AdminGamesPage() {
  const session = await getServerSession(authOptions);

  if (!session?.isAdmin) {
    return (
      <div className="mx-auto max-w-lg space-y-4 py-12">
        <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)]">
          Not authorized
        </h1>
        <p className="text-muted-foreground">This page is only available to site administrators.</p>
      </div>
    );
  }

  try {
    const api = await getServerApiClient();
    const [games, queue] = await Promise.all([
      api.getAdminGames(),
      api.getAdminGameTrackQueue(),
    ]);

    return (
      <div className="space-y-10">
        <div className="space-y-2">
          <h1 className="font-[family-name:var(--font-display)] text-2xl text-[var(--accent-retro)]">
            Manage games
          </h1>
          <p className="text-sm text-muted-foreground">
            Track RetroAchievements titles, add download mirrors, and refresh metadata for your group.
          </p>
        </div>

        <AdminGameTrackQueueSection items={queue} />

        <section className="space-y-3 rounded border border-border p-5">
          <h2 className="text-lg font-semibold">Add game</h2>
          <AddGameForm />
        </section>

        <AdminGamesSection games={games} />
      </div>
    );
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
}
