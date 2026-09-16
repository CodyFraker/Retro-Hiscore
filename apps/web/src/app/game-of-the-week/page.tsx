import Link from "next/link";
import { PageHero } from "@/components/layout/page-hero";
import { GameOfTheWeekView } from "@/components/game-of-the-week/game-of-the-week-view";
import { getServerApiClient } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function GameOfTheWeekPage() {
  try {
    const api = await getServerApiClient();
    const poll = await api.getGameOfTheWeekCurrent();
    return (
      <div className="space-y-8">
        <PageHero title="Game of the week" />
        <GameOfTheWeekView initialPoll={poll} />
      </div>
    );
  } catch {
    return (
      <div className="space-y-4">
        <PageHero title="Game of the week" />
        <p className="text-muted-foreground">No poll is active right now.</p>
        <p className="text-sm text-muted-foreground">
          Check back when an admin starts the next vote.
        </p>
        <Link href="/" className="text-sm text-[var(--accent-retro)] hover:underline">
          Back to dashboard
        </Link>
      </div>
    );
  }
}
