"use client";

import { Gamepad2 } from "lucide-react";
import { GameCard } from "@/components/dashboard/game-card";
import { TrackedGamesToolbar } from "@/components/dashboard/tracked-games-toolbar";
import { TrackedGamesPagination } from "@/components/dashboard/tracked-games-pagination";
import { Card, CardContent } from "@/components/ui/card";
import type { DashboardGamesResponse } from "@/generated/api-client";
import {
  type DashboardGameSortKey,
} from "@/lib/dashboard-games";
import { TRACKED_GAMES_PAGE_SIZE } from "@/lib/tracked-games-params";

type Props = {
  basePath: string;
  page: DashboardGamesResponse;
  query: string;
  sort: DashboardGameSortKey;
  showHeading?: boolean;
  showToolbar?: boolean;
};

export function TrackedGamesSection({
  basePath,
  page,
  query,
  sort,
  showHeading = true,
  showToolbar = true,
}: Props) {

  if (page.total === 0 && !query.trim()) {
    return (
      <Card id="tracked-games">
        <CardContent className="flex flex-col items-center gap-3 py-10 text-center">
          <Gamepad2 className="size-8 text-muted-foreground" />
          <div className="space-y-1">
            <p className="font-medium">No games tracked yet</p>
            <p className="max-w-sm text-sm text-muted-foreground">
              An administrator can add games from Admin, then run Refresh scores to pull friend
              leaderboards.
            </p>
          </div>
        </CardContent>
      </Card>
    );
  }

  const showHeaderRow = showHeading || showToolbar;

  return (
    <section id="tracked-games" className="space-y-3">
      {showHeaderRow ? (
        <div
          className={`flex flex-col gap-3 sm:flex-row sm:items-end ${showHeading ? "sm:justify-between" : "sm:justify-end"}`}
        >
          {showHeading ? (
            <div>
              <h2 className="steam-section-heading">Tracked games</h2>
              <p className="text-sm text-muted-foreground">
                {page.total} game{page.total === 1 ? "" : "s"}
                {query.trim() ? ` matching “${query.trim()}”` : ""}
              </p>
            </div>
          ) : null}
          {showToolbar ? (
            <TrackedGamesToolbar basePath={basePath} query={query} sort={sort} />
          ) : null}
        </div>
      ) : null}

      {page.items.length === 0 ? (
        <Card>
          <CardContent className="py-8 text-center text-sm text-muted-foreground">
            No games match your search.
          </CardContent>
        </Card>
      ) : (
        <Card className="py-0">
          <CardContent className="px-0">
            <ul className="divide-y divide-border">
              {page.items.map((game) => (
                <GameCard key={game.id} game={game} />
              ))}
            </ul>
            <TrackedGamesPagination
              basePath={basePath}
              total={page.total}
              offset={page.offset}
              limit={page.limit || TRACKED_GAMES_PAGE_SIZE}
              query={query}
              sort={sort}
            />
          </CardContent>
        </Card>
      )}
    </section>
  );
}
