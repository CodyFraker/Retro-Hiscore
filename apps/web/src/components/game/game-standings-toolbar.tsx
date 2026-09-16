"use client";

type Props = {
  query: string;
  friendScoresOnly: boolean;
  onQueryChange: (value: string) => void;
  onFriendScoresOnlyChange: (value: boolean) => void;
  onClearFilters: () => void;
  hasActiveFilters: boolean;
};

export function GameStandingsToolbar({
  query,
  friendScoresOnly,
  onQueryChange,
  onFriendScoresOnlyChange,
  onClearFilters,
  hasActiveFilters,
}: Props) {
  return (
    <div className="sticky top-0 z-10 -mx-4 space-y-3 border-b border-border bg-background/95 px-4 py-3 backdrop-blur supports-[backdrop-filter]:bg-background/80 sm:mx-0 sm:rounded-md sm:border sm:px-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <label className="flex min-w-0 flex-1 flex-col gap-1">
          <span className="text-xs font-medium text-muted-foreground">Search boards</span>
          <input
            type="search"
            value={query}
            onChange={(event) => onQueryChange(event.target.value)}
            placeholder="Title or description"
            className="h-8 w-full min-w-0 rounded-md bg-input px-2.5 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
          />
        </label>
        <label className="flex shrink-0 cursor-pointer items-center gap-2 text-sm">
          <input
            type="checkbox"
            checked={friendScoresOnly}
            onChange={(event) => onFriendScoresOnlyChange(event.target.checked)}
            className="size-4 rounded border-border"
          />
          Friend scores only
        </label>
      </div>
      {hasActiveFilters ? (
        <button
          type="button"
          onClick={onClearFilters}
          className="text-sm text-muted-foreground underline-offset-4 hover:text-foreground hover:underline"
        >
          Clear filters
        </button>
      ) : null}
    </div>
  );
}
