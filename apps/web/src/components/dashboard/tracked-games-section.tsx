"use client";

import { Gamepad2 } from "lucide-react";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { GameCard } from "@/components/dashboard/game-card";
import { TrackedGamesPagination } from "@/components/dashboard/tracked-games-pagination";
import { Card, CardContent } from "@/components/ui/card";
import type { DashboardGamesResponse } from "@/generated/api-client";
import {
  type DashboardGameSortKey,
} from "@/lib/dashboard-games";
import {
  buildTrackedGamesQueryString,
  TRACKED_GAMES_PAGE_SIZE,
} from "@/lib/tracked-games-params";

type Props = {
  basePath: string;
  page: DashboardGamesResponse;
  query: string;
  sort: DashboardGameSortKey;
  showHeading?: boolean;
};

const SORT_OPTIONS: { value: DashboardGameSortKey; label: string }[] = [
  { value: "recent", label: "Recent activity" },
  { value: "boards", label: "Board count" },
  { value: "population", label: "RA population" },
  { value: "name", label: "Name (A–Z)" },
];

export function TrackedGamesSection({
  basePath,
  page,
  query,
  sort,
  showHeading = true,
}: Props) {
  const router = useRouter();
  const [searchInput, setSearchInput] = useState(query);
  const [, startTransition] = useTransition();

  function navigate(next: { page?: number; q?: string; sort?: DashboardGameSortKey }) {
    const href = `${basePath}${buildTrackedGamesQueryString({
      page: next.page ?? 1,
      q: next.q ?? query,
      sort: next.sort ?? sort,
    })}`;
    startTransition(() => {
      router.push(href);
    });
  }

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

  return (
    <section id="tracked-games" className="space-y-3">
      {showHeading ? (
        <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <h2 className="steam-section-heading">Tracked games</h2>
            <p className="text-sm text-muted-foreground">
              {page.total} game{page.total === 1 ? "" : "s"}
              {query.trim() ? ` matching “${query.trim()}”` : ""}
            </p>
          </div>
        </div>
      ) : null}

      <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-end">
        <form
          className="flex flex-col gap-2 sm:flex-row sm:items-end"
          onSubmit={(event) => {
            event.preventDefault();
            navigate({ page: 1, q: searchInput });
          }}
        >
          <label className="flex flex-col gap-1">
            <span className="text-xs font-medium text-muted-foreground">Search</span>
            <input
              type="search"
              value={searchInput}
              onChange={(event) => setSearchInput(event.target.value)}
              placeholder="Title or platform"
              className="h-8 w-full min-w-0 rounded-md bg-input px-2.5 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50 sm:w-48"
            />
          </label>
          <button
            type="submit"
            className="h-8 rounded-md bg-secondary px-3 text-sm font-medium hover:bg-secondary/80"
          >
            Search
          </button>
        </form>
        <label className="flex flex-col gap-1">
          <span className="text-xs font-medium text-muted-foreground">Sort</span>
          <select
            value={sort}
            onChange={(event) =>
              navigate({ page: 1, sort: event.target.value as DashboardGameSortKey })
            }
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
