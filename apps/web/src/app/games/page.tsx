import { AlertCircle } from "lucide-react";
import { TrackedGamesSection } from "@/components/dashboard/tracked-games-section";
import { PageHero } from "@/components/layout/page-hero";
import { getServerApiClient } from "@/lib/api";
import {
  parseTrackedGamesPage,
  parseTrackedGamesSort,
  trackedGamesOffset,
  TRACKED_GAMES_PAGE_SIZE,
  type TrackedGamesSearchParams,
} from "@/lib/tracked-games-params";

export const dynamic = "force-dynamic";

type Props = {
  searchParams: Promise<TrackedGamesSearchParams>;
};

export default async function GamesIndexPage({ searchParams }: Props) {
  const params = await searchParams;
  const pageNum = parseTrackedGamesPage(params.page);
  const sort = parseTrackedGamesSort(params.sort);
  const query = params.q?.trim() ?? "";

  let gamesPage;
  let error: string | null = null;

  try {
    const api = await getServerApiClient();
    gamesPage = await api.getDashboardGames(
      TRACKED_GAMES_PAGE_SIZE,
      trackedGamesOffset(pageNum),
      sort,
      query || undefined,
    );
  } catch (err) {
    error = err instanceof Error ? err.message : "Failed to load games";
  }

  return (
    <div className="space-y-8">
      <PageHero
        title="Tracked games"
        titleClassName="text-2xl sm:text-3xl md:text-4xl"
        description="Friend leaderboard standings for every title your group tracks on RetroAchievements."
      />

      {error ? (
        <p className="flex items-center gap-2 rounded border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm">
          <AlertCircle className="size-4 shrink-0" />
          {error}
        </p>
      ) : (
        gamesPage && (
          <TrackedGamesSection
            basePath="/games"
            page={gamesPage}
            query={query}
            sort={sort}
            showHeading={false}
          />
        )
      )}
    </div>
  );
}
