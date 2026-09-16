"use client";

import { useMemo, useState } from "react";
import { GameStandingsSection } from "@/components/game/game-standings-section";
import { GameStandingsToolbar } from "@/components/game/game-standings-toolbar";
import type { GameLeaderboardDto, StandingMemberDto } from "@/generated/api-client";
import { applyLeaderboardFilters } from "@/lib/game-standings-filter";

type Props = {
  leaderboards: GameLeaderboardDto[];
  members: StandingMemberDto[];
};

export function GameStandingsWithFilters({ leaderboards, members }: Props) {
  const [query, setQuery] = useState("");
  const [friendScoresOnly, setFriendScoresOnly] = useState(true);

  const memberIds = useMemo(() => members.map((m) => m.id), [members]);

  const filtered = useMemo(
    () =>
      applyLeaderboardFilters(leaderboards, {
        query,
        friendScoresOnly,
        memberIds,
      }),
    [leaderboards, query, friendScoresOnly, memberIds],
  );

  const hasActiveFilters = query.trim().length > 0 || !friendScoresOnly;

  function clearFilters() {
    setQuery("");
    setFriendScoresOnly(true);
  }

  return (
    <div className="space-y-4">
      <GameStandingsToolbar
        query={query}
        friendScoresOnly={friendScoresOnly}
        onQueryChange={setQuery}
        onFriendScoresOnlyChange={setFriendScoresOnly}
        onClearFilters={clearFilters}
        hasActiveFilters={hasActiveFilters}
      />
      {filtered.length === 0 ? (
        <p className="text-sm text-muted-foreground">
          No boards match your filters. Try turning off friend scores only or clearing the search.
        </p>
      ) : (
        <GameStandingsSection leaderboards={filtered} members={members} />
      )}
    </div>
  );
}
