"use client";

import { Gamepad2 } from "lucide-react";
import { useMemo, useState } from "react";
import { GameCard } from "@/components/dashboard/game-card";
import { Card, CardContent } from "@/components/ui/card";
import type { DashboardGameDto } from "@/generated/api-client";
import {
  type DashboardGameSortKey,
  filterGamesByQuery,
  sortDashboardGames,
} from "@/lib/dashboard-games";

type Props = {
  games: DashboardGameDto[];
};

const SORT_OPTIONS: { value: DashboardGameSortKey; label: string }[] = [
  { value: "recent", label: "Recent activity" },
  { value: "boards", label: "Board count" },
  { value: "name", label: "Name (A–Z)" },
];

export function TrackedGamesSection({ games }: Props) {
  const [query, setQuery] = useState("");
  const [sortKey, setSortKey] = useState<DashboardGameSortKey>("recent");

  const visibleGames = useMemo(() => {
    const filtered = filterGamesByQuery(games, query);
    return sortDashboardGames(filtered, sortKey);
  }, [games, query, sortKey]);

  if (games.length === 0) {
    return (
      <Card id="tracked-games">
        <CardContent className="flex flex-col items-center gap-3 py-10 text-center">
          <Gamepad2 className="size-8 text-muted-foreground" />
          <div className="space-y-1">
            <p className="font-medium">No games tracked yet</p>
            <p className="max-w-sm text-sm text-muted-foreground">
              An administrator can add games from Manage Games, then run Refresh scores to pull friend
              leaderboards.
            </p>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <section id="tracked-games" className="space-y-3">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h2 className="steam-section-heading">Tracked games</h2>
          <p className="text-sm text-muted-foreground">
            {visibleGames.length} of {games.length} game{games.length === 1 ? "" : "s"}
          </p>
        </div>
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
          <label className="flex flex-col gap-1">
            <span className="text-xs font-medium text-muted-foreground">Search</span>
            <input
              type="search"
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              placeholder="Title or platform"
              className="h-8 w-full min-w-[12rem] rounded-md bg-input px-2.5 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50 sm:w-48"
            />
          </label>
          <label className="flex flex-col gap-1">
            <span className="text-xs font-medium text-muted-foreground">Sort</span>
            <select
              value={sortKey}
              onChange={(event) => setSortKey(event.target.value as DashboardGameSortKey)}
              className="h-8 rounded-md bg-input px-2.5 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
            >
              {SORT_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
        </div>
      </div>

      {visibleGames.length === 0 ? (
        <Card>
          <CardContent className="py-8 text-center text-sm text-muted-foreground">
            No games match your search.
          </CardContent>
        </Card>
      ) : (
        <Card className="py-0">
          <CardContent className="px-0">
            <ul className="divide-y divide-border">
              {visibleGames.map((game) => (
                <GameCard key={game.id} game={game} />
              ))}
            </ul>
          </CardContent>
        </Card>
      )}
    </section>
  );
}
