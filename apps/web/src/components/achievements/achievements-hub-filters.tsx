"use client";

import { useRouter } from "next/navigation";
import { useTransition } from "react";
import { buildAchievementsQueryString } from "@/lib/achievements-params";

export type AchievementsFilterGame = {
  raGameId: number;
  title: string;
};

export type AchievementsFilterMember = {
  raUsername: string;
  displayName: string;
};

type Props = {
  games: AchievementsFilterGame[];
  members: AchievementsFilterMember[];
  gameId?: number;
  member?: string;
};

export function AchievementsHubFilters({ games, members, gameId, member }: Props) {
  const router = useRouter();
  const [, startTransition] = useTransition();

  function navigate(next: { game?: number; member?: string }) {
    const href = `/achievements${buildAchievementsQueryString({
      page: 1,
      game: next.game,
      member: next.member,
    })}`;
    startTransition(() => {
      router.push(href);
    });
  }

  return (
    <div className="flex w-full flex-col gap-2 sm:flex-row sm:flex-wrap sm:items-end">
      <label className="flex w-full flex-col gap-1 sm:w-auto">
        <span className="text-xs font-medium text-muted-foreground">Game</span>
        <select
          value={gameId ?? ""}
          onChange={(event) => {
            const raw = event.target.value;
            navigate({
              game: raw ? Number.parseInt(raw, 10) : undefined,
              member,
            });
          }}
          className="h-8 w-full min-w-0 rounded-md bg-input px-2.5 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50 sm:min-w-[12rem]"
        >
          <option value="">All tracked games</option>
          {games.map((game) => (
            <option key={game.raGameId} value={game.raGameId}>
              {game.title}
            </option>
          ))}
        </select>
      </label>
      <label className="flex w-full flex-col gap-1 sm:w-auto">
        <span className="text-xs font-medium text-muted-foreground">Member</span>
        <select
          value={member ?? ""}
          onChange={(event) => {
            const raw = event.target.value;
            navigate({
              game: gameId,
              member: raw || undefined,
            });
          }}
          className="h-8 w-full min-w-0 rounded-md bg-input px-2.5 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50 sm:min-w-[12rem]"
        >
          <option value="">All members</option>
          {members.map((m) => (
            <option key={m.raUsername} value={m.raUsername}>
              {m.displayName}
            </option>
          ))}
        </select>
      </label>
    </div>
  );
}
