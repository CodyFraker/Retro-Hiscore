"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { type DashboardGameSortKey } from "@/lib/dashboard-games";
import { buildTrackedGamesQueryString } from "@/lib/tracked-games-params";

const SORT_OPTIONS: { value: DashboardGameSortKey; label: string }[] = [
  { value: "recent", label: "Recent activity" },
  { value: "boards", label: "Board count" },
  { value: "population", label: "RA population" },
  { value: "name", label: "Name (A–Z)" },
];

type Props = {
  basePath: string;
  query: string;
  sort: DashboardGameSortKey;
};

export function TrackedGamesToolbar({ basePath, query, sort }: Props) {
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

  return (
    <div className="flex shrink-0 flex-col gap-2 sm:flex-row sm:items-end">
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
  );
}
